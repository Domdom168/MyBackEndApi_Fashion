using System.ComponentModel.DataAnnotations;

namespace MyBackEndApi.DTOs.Order
{
    public class CreateOrderDto
    {
        [Required]
        public string CustomerName { get; set; }
        [Required]
        public string Phone { get; set; }
        [Required]
        public string Address { get; set; }
        [Required]
        public string PaymentMethod { get; set; }
        public string? BillNumber { get; set; }
        [Required]
        public List<OrderItemDto> Items { get; set; }
    }
    public class OrderItemDto
    {
        [Required]
        public int ProductId { get; set; }
        [Required]
        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }
        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Price { get; set; }
        public string? SelectedSize { get; set; }
        public string? SelectedColorName { get; set; }

    }

}
