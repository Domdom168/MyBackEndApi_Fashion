namespace MyBackEndApi.DTOs.Banner
{
    public class BannerResponseDto
    {
        public int Id { get; set; }
        public string? Title { get; set; }
        public string? ImageUrl { get; set; }
        public string? Link { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
