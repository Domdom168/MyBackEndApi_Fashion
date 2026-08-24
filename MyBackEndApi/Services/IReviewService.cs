using MyBackEndApi.DTOs.Review;

namespace MyBackEndApi.Services
{
    public interface IReviewService
    {
        Task<IEnumerable<ReviewResponseDto>> GetAllReviewsAsync();
        Task<ReviewResponseDto?> GetReviewByIdAsync(int id);
        Task<ReviewResponseDto> CreateReviewAsync(ReviewCreateDto dto, int userId);
        Task<ReviewResponseDto?> UpdateReviewApprovalAsync(int id, bool isApproved);
        Task<bool> DeleteReviewAsync(int id);
        Task<ProductRatingDto> GetProductReviewsAsync(int productId);
    }
}
