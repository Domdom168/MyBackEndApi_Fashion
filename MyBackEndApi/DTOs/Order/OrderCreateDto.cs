using System.ComponentModel.DataAnnotations;

namespace MyBackEndApi.DTOs.Order
{
    public class OrderCreateDto
    {
        public int? UserId { get; set; }
        public string? CustomerName { get; set; }
        public string? Phone { get; set; }
        public string? Address { get; set; }
        public string? PaymentMethod { get; set; }
        public List<OrderItemCreateDto> Items { get; set; } = new();
    }

    public class OrderItemCreateDto
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public string? SelectedSize { get; set; }
        public string? SelectedColorName { get; set; }
    }

}
