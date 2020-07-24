using System.Threading.Tasks;
using GoodBooks.BusinessLogic.Models;

namespace GoodBooks.BusinessLogic.Services
{
    public interface IReviewService
    {
        Task<ReviewModel> CreateReview(ReviewModel reviewModel);
    }
}