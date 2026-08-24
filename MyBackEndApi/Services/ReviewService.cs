using MyBackEndApi.Data;
using MyBackEndApi.DTOs.Review;
using MyBackEndApi.Models;
using Microsoft.EntityFrameworkCore;
namespace MyBackEndApi.Services
{
    public class ReviewService : IReviewService
    {
        private readonly AppDbContext _context;

        public ReviewService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ReviewResponseDto>> GetAllReviewsAsync()
        {
            return await _context.Reviews
                .Include(r => r.User)
                 .Include(r => r.Product)
                .OrderByDescending(r => r.CreatedAt)
                .Select(r => MapToDto(r))
                .ToListAsync();
        }

        public async Task<ReviewResponseDto?> GetReviewByIdAsync(int id)
        {
            var review = await _context.Reviews
                .Include(r => r.User)
                 .Include(r => r.Product)
                .FirstOrDefaultAsync(r => r.Id == id);
            return review == null ? null : MapToDto(review);
        }

        public async Task<ReviewResponseDto> CreateReviewAsync(ReviewCreateDto dto, int userId)
        {
            // Check if user already reviewed this product
            var existing = await _context.Reviews
                .FirstOrDefaultAsync(r => r.ProductId == dto.ProductId && r.UserId == userId);

            if (existing != null)
            {
                // If the existing review is approved, block new review
                if (existing.IsApproved)
                    throw new InvalidOperationException("You have already reviewed this product.");

                // Otherwise (pending or unapproved), remove the old review
                _context.Reviews.Remove(existing);
                await _context.SaveChangesAsync(); // save deletion before creating new one
            }

            // Create new review
            var review = new Review
            {
                ProductId = dto.ProductId,
                UserId = userId,
                Rating = dto.Rating,
                Comment = dto.Comment,
                IsApproved = false, // pending approval
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Reviews.Add(review);
            await _context.SaveChangesAsync();

            // Update product rating (only approved reviews count)
            await UpdateProductRatingAsync(dto.ProductId);

            return MapToDto(review);
        }

        public async Task<ReviewResponseDto?> UpdateReviewApprovalAsync(int id, bool isApproved)
        {
            var review = await _context.Reviews.FindAsync(id);
            if (review == null) return null;

            review.IsApproved = isApproved;
            review.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            // Update product rating
            await UpdateProductRatingAsync(review.ProductId);

            return await GetReviewByIdAsync(id);
        }

        public async Task<bool> DeleteReviewAsync(int id)
        {
            var review = await _context.Reviews.FindAsync(id);
            if (review == null) return false;

            var productId = review.ProductId;
            _context.Reviews.Remove(review);
            await _context.SaveChangesAsync();

            // Update product rating
            await UpdateProductRatingAsync(productId);

            return true;
        }

        public async Task<ProductRatingDto> GetProductReviewsAsync(int productId)
        {
            var approvedReviews = await _context.Reviews
                .Where(r => r.ProductId == productId && r.IsApproved)
                .Include(r => r.User)
                 .Include(r => r.Product)
                .OrderByDescending(r => r.CreatedAt)
                .Select(r => MapToDto(r))
                .ToListAsync();

            var average = approvedReviews.Any()
                ? approvedReviews.Average(r => r.Rating)
                : 0;

            return new ProductRatingDto
            {
                AverageRating = (decimal)Math.Round(average, 1),
                TotalReviews = approvedReviews.Count,
                Reviews = approvedReviews
            };
        }

        private async Task UpdateProductRatingAsync(int productId)
        {
            var approvedReviews = await _context.Reviews
                .Where(r => r.ProductId == productId && r.IsApproved)
                .ToListAsync();

            var average = approvedReviews.Any()
                ? approvedReviews.Average(r => r.Rating)
                : 0;

            var product = await _context.Products.FindAsync(productId);
            if (product != null)
            {
                product.Rating = (decimal)Math.Round(average, 1);
                product.Reviews = approvedReviews.Count;
                await _context.SaveChangesAsync();
            }
        }

        private static ReviewResponseDto MapToDto(Review r) => new()
        {
            Id = r.Id,
            ProductId = r.ProductId,
            ProductName = r.Product != null ? (r.Product.NameEnglish ?? r.Product.NameKhmer) : null,
            UserId = r.UserId,
            UserName = r.User?.Name,
            Rating = r.Rating,
            Comment = r.Comment,
            IsApproved = r.IsApproved,
            CreatedAt = r.CreatedAt,
            UpdatedAt = r.UpdatedAt
        };
    }
}
