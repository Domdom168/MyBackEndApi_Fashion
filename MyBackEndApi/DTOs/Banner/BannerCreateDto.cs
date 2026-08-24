namespace MyBackEndApi.DTOs.Banner
{
    public class BannerCreateDto
    {
        public string? Title { get; set; }
        public IFormFile? ImageFile { get; set; }
        public string? Link { get; set; }
        public int DisplayOrder { get; set; } = 0;
        public bool IsActive { get; set; } = true;
    }
}
