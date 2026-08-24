using MyBackEndApi.DTOs.ProductVariant;

namespace MyBackEndApi.DTOs.Product
{
    public class ProductCreateDto
    {
        public int CategoryId { get; set; }
        public string? NameKhmer { get; set; }
        public string? NameEnglish { get; set; }
        public string? DescriptionKhmer { get; set; }
        public string? DescriptionEnglish { get; set; }
        public decimal Price { get; set; }
        public decimal? Rating { get; set; }
        public IFormFile? ImageFile { get; set; }
        public decimal? DiscountedPrice { get; set; }
        public string? DiscountType { get; set; }
        public decimal? DiscountValue { get; set; }
        public DateTime? DiscountStartDate { get; set; }
        public DateTime? DiscountEndDate { get; set; }
        public List<IFormFile>? AdditionalImages { get; set; }
        public List<string>? Colors { get; set; }
        public List<string>? Sizes { get; set; }
        public List<VariantCreateDto>? Variants { get; set; }
    }
}