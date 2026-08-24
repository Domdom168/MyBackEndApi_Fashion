namespace MyBackEndApi.DTOs.Review
{
    public class ProductRatingDto
    {
        public decimal AverageRating { get; set; }
        public int TotalReviews { get; set; }
        public List<ReviewResponseDto> Reviews { get; set; } = new();
    }
}
