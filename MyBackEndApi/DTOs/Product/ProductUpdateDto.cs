namespace MyBackEndApi.DTOs.Product
{
    public class ProductUpdateDto
    {
        public int CategoryId { get; set; }
        public string? NameKhmer { get; set; }
        public string? NameEnglish { get; set; }
        public string? DescriptionKhmer { get; set; }
        public string? DescriptionEnglish { get; set; }
        public decimal Price { get; set; }
        public decimal? Rating { get; set; }
    }
}
