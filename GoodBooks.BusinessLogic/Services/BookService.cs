using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using GoodBooks.BusinessLogic.Csv;
using GoodBooks.BusinessLogic.Models;
using GoodBooks.Common.Exceptions;
using GoodBooks.Common.Pagination;
using GoodBooks.Data.Model;
using GoodBooks.Data.Model.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Nest;

namespace GoodBooks.BusinessLogic.Services
{
    public class BookService : IBookService
    {
        private readonly GoodBooksContext context;
        private readonly ICsvParserService csvParserService;
        private readonly IMapper mapper;
        private readonly IElasticClient elasticClient;
        private readonly ILogger logger;

        public BookService(GoodBooksContext context,
            ICsvParserService csvParserService,
            IMapper mapper,
            IElasticClient elasticClient,
            ILogger<BookService> logger)
        {
            this.context = context;
            this.csvParserService = csvParserService;
            this.mapper = mapper;
            this.elasticClient = elasticClient;
            this.logger = logger;
        }

        public async Task<PaginationResult<BookModelExtended>> FindBooks(PaginationParameters paginationParameters,
            string searchQuery = null)
        {
            if (paginationParameters == null)
                throw new ArgumentNullException(nameof(paginationParameters));

            var searchRequest = new SearchRequest<Book>(Indices.Index<Book>())
            {
                From = paginationParameters.From,
                Size = paginationParameters.Size,
                Query = new MultiMatchQuery
                {
                    Fields = Infer.Field<Book>(f => f.Title, 2).And<Book>(f => f.Authors, 1),
                    Query = searchQuery
                }
            };

            var booksResponse = await elasticClient.SearchAsync<Book>(searchRequest);

            if (booksResponse.Total <= 0)
                return new PaginationResult<BookModelExtended>();

            var books = await GetExtendedBookModels(booksResponse.Documents);

            return new PaginationResult<BookModelExtended>(books, booksResponse.Total);
        }

        public async Task<BookModelExtended> GetBook(int id)
        {
            var elasticBookResponse = await elasticClient.GetAsync(new DocumentPath<Book>(id));

            if (!elasticBookResponse.Found)
                throw new EntityNotFoundException($"Could not get book with id = {id}. Book wasn't found.");

            return await GetExtendedBookModel(elasticBookResponse.Source);
        }

        public async Task<BookModelExtended> CreateBook(BookModel bookModel)
        {
            if (bookModel == null)
                throw new ArgumentNullException(nameof(bookModel));

            var book = mapper.Map<Book>(bookModel);

            await context.Books.AddAsync(book);
            await context.SaveChangesAsync();

            await elasticClient.IndexDocumentAsync(book);

            return await GetExtendedBookModel(book);
        }

        public async Task<BookModelExtended> UpdateBook(int id, BookModel bookModel)
        {
            if (bookModel == null)
                throw new ArgumentNullException(nameof(bookModel));

            var bookForUpdate = await context.Books.SingleOrDefaultAsync(b => b.BookId == id);
            if (bookForUpdate == null)
                throw new EntityNotFoundException($"Error updating book with id = {id}. Book wasn't found.");

            mapper.Map(bookModel, bookForUpdate);
            context.Entry(bookForUpdate).State = EntityState.Modified;

            await context.SaveChangesAsync();

            var elasticBookResponse = await elasticClient.GetAsync(new DocumentPath<Book>(bookForUpdate.BookId));
            await elasticClient.UpdateAsync<Book>(elasticBookResponse.Source, u => u.Doc(bookForUpdate));

            return await GetExtendedBookModel(bookForUpdate);
        }

        public async Task<BookModelExtended> DeleteBook(int id)
        {
            var book = await context.Books.FindAsync(id);
            if (book == null)
                throw new EntityNotFoundException($"Could not delete a book. Book with id {id} doesn't exist.");

            var deletedBook = await GetExtendedBookModel(book);

            context.Books.Remove(book);
            await context.SaveChangesAsync();

            await elasticClient.DeleteAsync<Book>(book);

            return deletedBook;

        }

        public async Task BulkAdd(Stream booksStream)
        {
            if (booksStream == null)
                throw new ArgumentNullException(nameof(booksStream));

            if (booksStream.Length == 0)
                throw new ArgumentException($"An empty stream was passed for books bulk insert.");

            var bookModels = csvParserService.Parse<BookModel, BookModelMap>(booksStream);
            var books = mapper.Map<IEnumerable<Book>>(bookModels).ToList();

            await context.BulkInsertAsync(books);

            await elasticClient.IndexManyAsync(books);

            logger.LogInformation($"Bulk insert of {books.Count} books has successfully finished at {DateTime.UtcNow}.");
        }

        public async Task ResetLibrary()
        {
            await context.Books.BulkDeleteAsync(context.Books);
            await elasticClient.Indices.DeleteAsync(Indices.Index<Book>());
        }

        private async Task<IEnumerable<BookModelExtended>> GetExtendedBookModels(IEnumerable<Book> books)
        {
            if (books == null)
                throw new ArgumentNullException(nameof(books));

            context.AttachRange(books);
            foreach (var book in books)
            {
                await context.Entry(book).Collection(b => b.Reviews).LoadAsync();
            }

            return mapper.Map<IEnumerable<BookModelExtended>>(books);
        }

        private async Task<BookModelExtended> GetExtendedBookModel(Book book) =>
            (await GetExtendedBookModels(new[] {book})).Single();
    }
}