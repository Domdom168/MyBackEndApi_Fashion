namespace MyBackEndApi.DTOs.ProductVariant
{
    public class VariantUpdateDto
    {
        public string? Size { get; set; }
        public string? ColorName { get; set; }
        public string? ColorHex { get; set; }
        public int Stock { get; set; }
        public decimal? Price { get; set; }
    }
}
