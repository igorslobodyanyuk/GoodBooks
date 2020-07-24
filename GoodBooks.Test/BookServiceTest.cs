using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using GoodBooks.Api.AutoMapperProfiles;
using GoodBooks.BusinessLogic.Csv;
using GoodBooks.BusinessLogic.Models;
using GoodBooks.BusinessLogic.Services;
using GoodBooks.Common.Exceptions;
using GoodBooks.Common.Pagination;
using GoodBooks.Data.Model;
using GoodBooks.Data.Model.Models;
using GoodBooks.Test.Stubs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Nest;
using Xunit;
using Z.EntityFramework.Extensions;

namespace GoodBooks.Test
{
    public class BookServiceTest : IDisposable
    {
        #region Setup

        private readonly Mock<ICsvParserService> csvParserServiceMock;
        private readonly Mock<IElasticClient> elasticClientMock;
        private readonly Mock<ILogger<BookService>> loggerMock;

        private readonly GoodBooksContext goodBooksContext;
        private readonly IMapper mapper;

        private readonly IBookService bookService;

        public BookServiceTest()
        {
            csvParserServiceMock = new Mock<ICsvParserService>();
            elasticClientMock = new Mock<IElasticClient>();
            loggerMock = new Mock<ILogger<BookService>>();

            goodBooksContext = GetDbContext();
            mapper = GetMapper();

            bookService = new BookService(goodBooksContext, csvParserServiceMock.Object, mapper,
                elasticClientMock.Object, loggerMock.Object);
        }

        public void Dispose()
        {
            goodBooksContext.Database.EnsureDeleted();
        }

        #endregion

        #region Find

        [Fact]
        public async Task FindBooksWithNullArgument_ThrowsException()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => bookService.FindBooks(null));
        }

        [Fact]
        public async Task FindExistingBooksWithoutQueryAndReviews_ReturnsBooksFromElastic()
        {
            var existingBooks = new[]
            {
                new Book {Title = "Foundation", Authors = "Isaac Asimov"},
                new Book {Title = "I, Robot", Authors = "Isaac Asimov"}
            };

            var mockSearchResponse = new Mock<ISearchResponse<Book>>();
            mockSearchResponse.Setup(x => x.Documents).Returns(existingBooks);
            elasticClientMock.Setup(client => client.SearchAsync<Book>(It.IsAny<SearchRequest<Book>>(), default)).ReturnsAsync(
                mockSearchResponse.Object);

            var foundBooks = await bookService.FindBooks(new PaginationParameters());

            Assert.NotNull(foundBooks);
            Assert.Equal(existingBooks.Length, foundBooks.Items.Count());
            Assert.Equal(existingBooks.First().Title, foundBooks.Items.First().Title);

            elasticClientMock.Verify(c => c.SearchAsync<Book>(It.IsAny<SearchRequest<Book>>(), default), Times.Once);
        }

        #endregion

        #region Get

        [Fact]
        public async Task GetNonExistingBook_ThrowsException()
        {
            elasticClientMock.Setup(client => client.GetAsync(It.IsAny<DocumentPath<Book>>(), default, default)).ReturnsAsync(
                new GetResponseStub<Book>(found: false));

            await Assert.ThrowsAsync<EntityNotFoundException>(() => bookService.GetBook(1));
        }

        [Fact]
        public async Task GetExistingBook_ReturnsBookFromElastic()
        {
            var id = 1;
            var existingBook = new Book {BookId = id, Title = "Foundation", Authors = "Isaac Asimov"};

            elasticClientMock.Setup(client => client.GetAsync(It.IsAny<DocumentPath<Book>>(), default, default)).ReturnsAsync(
                new GetResponseStub<Book>(found: true, source: existingBook));

            var foundBook = await bookService.GetBook(id);

            Assert.NotNull(foundBook);
            Assert.Equal(id, foundBook.BookId);
            Assert.Equal(existingBook.Title, foundBook.Title);

            elasticClientMock.Verify(c => c.GetAsync(It.IsAny<DocumentPath<Book>>(), default, default), Times.Once);
        }

        #endregion

        #region Create

        [Fact]
        public async Task CreateBookWithNullArgument_ThrowsException()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => bookService.CreateBook(null));
        }

        [Fact]
        public async Task CreateValidBook_CreatesAndIndexesAndReturnsCreatedBook()
        {
            var book = new BookModel("Foundation", "Isaac Asimov");

            var createdBook = await bookService.CreateBook(book);

            Assert.NotNull(createdBook);
            Assert.Equal(book.Title, createdBook.Title);
            Assert.True(createdBook.BookId > 0);

            var bookInContext = goodBooksContext.Books.SingleOrDefault();
            Assert.NotNull(bookInContext);
            Assert.Equal(book.Title, bookInContext.Title);
            
            elasticClientMock.Verify(c => c.IndexDocumentAsync(It.IsAny<Book>(), default),
                Times.Once);
        }

        #endregion

        #region Update

        [Fact]
        public async Task UpdateBookWithNullArgument_ThrowsException()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => bookService.UpdateBook(10, null));
        }

        [Fact]
        public async Task UpdateNonExistingBook_ThrowsException()
        {
            await Assert.ThrowsAsync<EntityNotFoundException>(() =>
                bookService.UpdateBook(10, new BookModel("Foundation", "Isaac Asimov")));
        }

        [Fact]
        public async Task UpdateExistingBook_UpdatesBook()
        {
            var bookForUpdate = new BookModel("Foundation v2", "Isaac Asimov");
            var existingBook = new Book {Title = "Foundation", Authors = "Isaac Asimov"};

            await goodBooksContext.Books.AddAsync(existingBook);
            await goodBooksContext.SaveChangesAsync();

            elasticClientMock.Setup(client => client.GetAsync(It.IsAny<DocumentPath<Book>>(), default, default)).ReturnsAsync(
                new GetResponseStub<Book>(found: true, source: existingBook));

            var updatedBook = await bookService.UpdateBook(existingBook.BookId, bookForUpdate);

            Assert.NotNull(updatedBook);
            Assert.Equal(existingBook.BookId, updatedBook.BookId);
            Assert.Equal(bookForUpdate.Title, updatedBook.Title);

            var bookInContext = goodBooksContext.Books.SingleOrDefault();
            Assert.NotNull(bookInContext);
            Assert.Equal(existingBook.BookId, bookInContext.BookId);
            Assert.Equal(bookForUpdate.Authors, bookInContext.Authors);
            Assert.Equal(bookForUpdate.Title, bookInContext.Title);

            elasticClientMock.Verify(
                c => c.UpdateAsync<Book>(It.IsAny<DocumentPath<Book>>(),
                    It.IsAny<Func<UpdateDescriptor<Book, Book>, IUpdateRequest<Book, Book>>>(), default), Times.Once);
        }

        #endregion

        #region Delete

        [Fact]
        public async Task DeleteNonExistingBook_ThrowsException()
        {
            await Assert.ThrowsAsync<EntityNotFoundException>(() => bookService.DeleteBook(10));
        }

        [Fact]
        public async Task DeleteExistingBook_DeletesBookReturnsDeleted()
        {
            var existingBook = new Book {Title = "Foundation", Authors = "Isaac Asimov"};

            await goodBooksContext.Books.AddAsync(existingBook);
            await goodBooksContext.SaveChangesAsync();

            var deletedBook = await bookService.DeleteBook(existingBook.BookId);

            Assert.Equal(existingBook.BookId, deletedBook.BookId);
            Assert.Equal(existingBook.Title, deletedBook.Title);

            Assert.False(goodBooksContext.Books.Any());

            elasticClientMock.Verify(
                c => c.DeleteAsync(It.IsAny<DocumentPath<Book>>(), default, default), Times.Once);
        }

        [Fact]
        public async Task DeleteExistingBookWithReviews_DeletesBookAndReviewsReturnsDeleted()
        {
            var existingBook = new Book {Title = "Foundation", Authors = "Isaac Asimov", Reviews = new List<Review>
                {
                    new Review {Email = "test1@gmail.com", Text = "Awesome book!"},
                    new Review {Email = "test2@gmail.com", Text = "Must read for any SciFi fun"}
                }
            };

            await goodBooksContext.Books.AddAsync(existingBook);
            await goodBooksContext.SaveChangesAsync();

            var deletedBook = await bookService.DeleteBook(existingBook.BookId);

            Assert.Equal(existingBook.Title, deletedBook.Title);
            Assert.False(goodBooksContext.Books.Any());
            Assert.False(goodBooksContext.Reviews.Any());

            elasticClientMock.Verify(
                c => c.DeleteAsync(It.IsAny<DocumentPath<Book>>(), default, default), Times.Once);
        }

        #endregion

        #region BulkInsert

        [Fact]
        public async Task BulkInsertBooksWithNullArgument_ThrowsException()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => bookService.BulkAdd(null));
        }

        [Fact]
        public async Task BulkInsertBooksEmptyStream_ThrowsException()
        {
            await Assert.ThrowsAsync<ArgumentException>(() => bookService.BulkAdd(Stream.Null));
        }

        [Fact]
        public async Task BulkInsertValidBooks_CreatesAndIndexesBooks()
        {
            var existingBooks = new[]
            {
                new BookModel("Foundation", "Isaac Asimov"),
                new BookModel("I, Robot", "Isaac Asimov")
            };
            var booksStream = GetStream(existingBooks);

            csvParserServiceMock.Setup(x => x.Parse<BookModel, BookModelMap>(It.IsAny<Stream>())).Returns(existingBooks);

            await bookService.BulkAdd(booksStream);

            var booksInDb = goodBooksContext.Books.ToList();
            Assert.Equal(existingBooks.Length, booksInDb.Count);
            Assert.Equal(existingBooks.First().Title, booksInDb.First().Title);
        }

        #endregion

        #region Helpers

        private static IMapper GetMapper()
        {
            var config = new MapperConfiguration(opts =>
            {
                opts.AddProfile<BookProfile>();
                opts.AddProfile<CommonProfile>();
                opts.AddProfile<ReviewProfile>();
            });
            var mapper = config.CreateMapper();
            return mapper;
        }

        private GoodBooksContext GetDbContext()
        {
            var option = new DbContextOptionsBuilder<GoodBooksContext>()
                .UseInMemoryDatabase("test-database").Options;
            var context = new GoodBooksContext(option);

            context.Database.EnsureCreated();

            EntityFrameworkManager.ContextFactory = _ => context;

            return context;
        }

        private Stream GetStream<T>(T item)
        {
            return new MemoryStream(Encoding.Unicode.GetBytes(Newtonsoft.Json.JsonConvert.SerializeObject(item)));
        }

        #endregion
    }
}