namespace MyBackEndApi.DTOs.Cart
{
    public class CartItemDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string? ProductName { get; set; }
        public string? ProductImage { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public string? SelectedSize { get; set; }
        public string? SelectedColorName { get; set; }
        public decimal Subtotal => Price * Quantity;
    }
}
