namespace MyBackEndApi.DTOs.Color
{
    public class ProductColorDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string? ColorName { get; set; }
        public string? ColorHex { get; set; }
    }
}
