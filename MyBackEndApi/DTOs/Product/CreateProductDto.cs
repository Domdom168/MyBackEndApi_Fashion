using System.ComponentModel.DataAnnotations;

namespace MyBackEndApi.DTOs.Product
{
    public class CreateProductDto
    {
        // Basic product info
        // Name – you can provide English or Khmer; we'll map to English if given
        [Required(ErrorMessage = "Product name is required")]
        public string? NameKhmer { get; set; }
        [Required(ErrorMessage = "Product name is required")]
        public string? NameEnglish { get; set; }
        // Description fields
        public string? DescriptionKhmer { get; set; }
        public string? DescriptionEnglish { get; set; }

        // Category ID – required (you need to know the category ID from your UI)
        public int CategoryId { get; set; }

        // Pricing & stock
        public decimal Price { get; set; }
        public int Stock { get; set; }

        // Rating – optional, default 0
        public decimal? Rating { get; set; }

        // Main image file
        public IFormFile? ImageFile { get; set; }

        // Optional extras (if you want to support colors, sizes, extra images)
        public List<IFormFile>? AdditionalImages { get; set; }
        public List<string>? Colors { get; set; }
        public List<string>? Sizes { get; set; }
    }
}
