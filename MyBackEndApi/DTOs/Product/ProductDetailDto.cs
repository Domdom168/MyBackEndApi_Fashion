using MyBackEndApi.DTOs.ProductVariant;

namespace MyBackEndApi.DTOs.Product
{
    public class ProductDetailDto
    {
        public int Id { get; set; }
        public string? ImageUrl { get; set; }
        public string? NameKhmer { get; set; }
        public string? NameEnglish { get; set; }
        public string? DescriptionKhmer { get; set; }
        public string? DescriptionEnglish { get; set; }
        public int? CategoryId { get; set; }
        public string? CategoryName { get; set; }
        public decimal? Price { get; set; }
        public int? Stock { get; set; }
        public decimal? Rating { get; set; }
        public decimal? DiscountedPrice { get; set; }
        public string? DiscountType { get; set; }
        public decimal? DiscountValue { get; set; }
        public DateTime? DiscountStartDate { get; set; }
        public DateTime? DiscountEndDate { get; set; }
        public List<ProductImageDto> Images { get; set; } = new();
        public List<VariantResponseDto> Variants { get; set; } = new();
        public List<string> Colors { get; set; } = new();
        public List<string> Sizes { get; set; } = new();
    }
}
