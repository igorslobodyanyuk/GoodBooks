using System;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using GoodBooks.Api.AutoMapperProfiles;
using GoodBooks.BusinessLogic.Models;
using GoodBooks.BusinessLogic.Services;
using GoodBooks.Common.Exceptions;
using GoodBooks.Data.Model;
using GoodBooks.Data.Model.Models;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace GoodBooks.Test
{
    public class ReviewServiceTest : IDisposable
    {

        #region Setup
        
        private readonly GoodBooksContext goodBooksContext;
        private readonly IMapper mapper;

        private readonly IReviewService reviewService;

        private DbConnection dbConnection;

        public ReviewServiceTest()
        {
            goodBooksContext = GetDbContext();
            mapper = GetMapper();

            reviewService = new ReviewService(goodBooksContext, mapper);
        }

        public void Dispose()
        {
            goodBooksContext.Database.EnsureDeleted();
            dbConnection.Close();
        }

        #endregion

        #region Create

        [Fact]
        public async Task CreateReviewWithNullArgument_ThrowsException()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => reviewService.CreateReview(null));
        }

        [Fact]
        public async Task CreateReviewForNonExistingBook_ThrowsException()
        {
            var review = new ReviewModel(1, "Great book!", "test@gmail.com");

            await Assert.ThrowsAsync<EntityNotFoundException>(() => reviewService.CreateReview(review));
        }

        [Fact]
        public async Task CreateReviewForExistingBook_CreatesReviewForBookAndReturns()
        {
            var existingBook = new Book{Authors = "Isaac Asimov", Title = "Foundation"};
            await goodBooksContext.Books.AddAsync(existingBook);
            await goodBooksContext.SaveChangesAsync();

            var newReview = new ReviewModel(existingBook.BookId, "Great book!", "test@gmail.com");

            var createdReview = await reviewService.CreateReview(newReview);
            
            Assert.NotNull(createdReview);
            Assert.Equal(newReview.Text, createdReview.Text);
            Assert.Equal(newReview.Email, createdReview.Email);

            Assert.Equal(1, goodBooksContext.Reviews.Count());

            var review = goodBooksContext.Reviews.Single();
            Assert.Equal(newReview.Text, review.Text);
            Assert.Equal(existingBook.BookId, review.BookId);
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
            dbConnection = new SqliteConnection("DataSource=:memory:");
            dbConnection.Open();

            var option = new DbContextOptionsBuilder<GoodBooksContext>()
                .UseSqlite(dbConnection).Options;
            var context = new GoodBooksContext(option);

            context.Database.EnsureCreated();

            return context;
        }

        #endregion
    }
}