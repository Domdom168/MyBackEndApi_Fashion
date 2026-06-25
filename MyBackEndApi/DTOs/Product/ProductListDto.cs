namespace MyBackEndApi.DTOs.Product
{
    public class ProductListDto
    {
        public int Id { get; set; }
        public int CategoryId { get; set; }
        public string? ImageUrl { get; set; }          // from product_images (first image)
        public string? NameKhmer { get; set; }              // combine or pick English name
        public string NameEnglish { get; set; }
        public string? CategoryKhmer { get; set; }      // from categories.name_english
        public string? CategoryEnglish { get; set; }
        public string DescriptionKhmer { get; set; } = "";         // from product_details.description
        public string DescriptionEnglish { get; set; } = "";
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public decimal Rating { get; set; }
        public string? Service { get; set; }           // placeholder, e.g., "Available"
    }
}
