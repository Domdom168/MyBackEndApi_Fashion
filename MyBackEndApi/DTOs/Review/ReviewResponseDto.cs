namespace MyBackEndApi.DTOs.Review
{
    public class ReviewResponseDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string? ProductName { get; set; }
        public int UserId { get; set; }
        public string? UserName { get; set; }
        public int Rating { get; set; }
        public string? Comment { get; set; }
        public bool IsApproved { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
