using System.Collections.Generic;
using System.Threading.Tasks;
using GoodBooks.BusinessLogic.Models;
using GoodBooks.BusinessLogic.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GoodBooks.Data.Model;
using GoodBooks.Data.Model.Models;

namespace GoodBooks.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReviewsController : ControllerBase
    {
        private readonly IReviewService reviewService;

        public ReviewsController(IReviewService reviewService)
        {
            this.reviewService = reviewService;
        }
        
        // POST: api/Reviews
        [HttpPost]
        public async Task<IActionResult> PostReview(ReviewModel review)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            await reviewService.CreateReview(review);

            return Ok();
        }
    }
}
