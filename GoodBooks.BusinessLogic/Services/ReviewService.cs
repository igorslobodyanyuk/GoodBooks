using System;
using System.Threading.Tasks;
using AutoMapper;
using GoodBooks.BusinessLogic.Models;
using GoodBooks.Common.Exceptions;
using GoodBooks.Data.Model;
using GoodBooks.Data.Model.Models;

namespace GoodBooks.BusinessLogic.Services
{
    public class ReviewService : IReviewService
    {
        private readonly GoodBooksContext context;
        private readonly IMapper mapper;

        public ReviewService(GoodBooksContext context, IMapper mapper)
        {
            this.context = context;
            this.mapper = mapper;
        }

        public async Task<ReviewModel> CreateReview(ReviewModel reviewModel)
        {
            if (reviewModel == null)
                throw new ArgumentNullException(nameof(reviewModel));

            var book = await context.Books.FindAsync(reviewModel.BookId) ??
                throw new EntityNotFoundException($"Cannot create a review. Book with id {reviewModel.BookId} doesn't exist.");

            var review = mapper.Map<Review>(reviewModel);
            review.Book = book;

            await context.Reviews.AddAsync(review);
            
            await context.SaveChangesAsync();

            return mapper.Map<ReviewModel>(review);
        }
    }
}