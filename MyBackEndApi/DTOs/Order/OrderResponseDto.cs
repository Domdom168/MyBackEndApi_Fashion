namespace MyBackEndApi.DTOs.Order
{
    public class OrderResponseDto
    {
        public int Id { get; set; }
        public int? UserId { get; set; }
        public string? CustomerName { get; set; }
        public string? Phone { get; set; }
        public string? Address { get; set; }
        public decimal TotalAmount { get; set; }
        public string? Status { get; set; }
        public string? PaymentMethod { get; set; }
        public string? CashierName { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<OrderItemDto> Items { get; set; } = new();
    }
    public class OrderItemDto
    {
        public int ProductId { get; set; }
        public string? ProductName { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public string? SelectedSize { get; set; }
        public string? SelectedColorName { get; set; }
    }
}
