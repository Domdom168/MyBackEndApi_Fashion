namespace MyBackEndApi.DTOs.Banner
{
    public class BannerUpdateDto
    {
        public string? Title { get; set; }
        public IFormFile? ImageFile { get; set; }
        public string? Link { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; }
    }
}
