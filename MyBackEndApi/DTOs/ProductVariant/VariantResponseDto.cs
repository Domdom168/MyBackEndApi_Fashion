namespace MyBackEndApi.DTOs.ProductVariant
{
    public class VariantResponseDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string? Size { get; set; }
        public string? ColorName { get; set; }
        public string? ColorHex { get; set; }
        public int Stock { get; set; }
        public decimal? Price { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
