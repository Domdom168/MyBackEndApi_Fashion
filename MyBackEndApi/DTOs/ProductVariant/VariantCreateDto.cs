namespace MyBackEndApi.DTOs.ProductVariant
{
    public class VariantCreateDto
    {
        public int ProductId { get; set; }
        public string? Size { get; set; }
        public string? ColorName { get; set; }
        public string? ColorHex { get; set; }
        public int Stock { get; set; } = 0;
        public decimal? Price { get; set; }
    }
}
