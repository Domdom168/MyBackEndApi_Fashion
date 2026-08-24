using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyBackEndApi.DTOs.Review;
using MyBackEndApi.Services;
using System.Security.Claims;

namespace MyBackEndApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReviewsController : ControllerBase
    {
        private readonly IReviewService _reviewService;

        public ReviewsController(IReviewService reviewService)
        {
            _reviewService = reviewService;
        }

        // PUBLIC: get approved reviews for a product (with average rating)
        [HttpGet("product/{productId}")]
        public async Task<ActionResult<ProductRatingDto>> GetProductReviews(int productId)
        {
            var result = await _reviewService.GetProductReviewsAsync(productId);
            return Ok(result);
        }

        // ADMIN: get all reviews (including pending)
        [HttpGet]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult<IEnumerable<ReviewResponseDto>>> GetAllReviews()
        {
            var reviews = await _reviewService.GetAllReviewsAsync();
            return Ok(reviews);
        }

        // ADMIN: get review by id
        [HttpGet("{id}")]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult<ReviewResponseDto>> GetReviewById(int id)
        {
            var review = await _reviewService.GetReviewByIdAsync(id);
            if (review == null) return NotFound();
            return Ok(review);
        }

        // CUSTOMER: submit a review (requires login)
        [HttpPost]
        [Authorize]
        public async Task<ActionResult<ReviewResponseDto>> CreateReview([FromBody] ReviewCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                if (userId == 0) return Unauthorized();

                var review = await _reviewService.CreateReviewAsync(dto, userId);
                return CreatedAtAction(nameof(GetReviewById), new { id = review.Id }, review);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // ADMIN: approve or hide a review
        [HttpPut("{id}/approval")]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult<ReviewResponseDto>> UpdateApproval(int id, [FromBody] ReviewUpdateDto dto)
        {
            var review = await _reviewService.UpdateReviewApprovalAsync(id, dto.IsApproved);
            if (review == null) return NotFound();
            return Ok(review);
        }

        // ADMIN: delete a review
        [HttpDelete("{id}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> DeleteReview(int id)
        {
            var deleted = await _reviewService.DeleteReviewAsync(id);
            if (!deleted) return NotFound();
            return NoContent();
        }
    }
}