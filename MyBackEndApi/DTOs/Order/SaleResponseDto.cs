namespace MyBackEndApi.DTOs.Order
{
    public class SaleResponseDto
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public int? CashierId { get; set; }
        public string? CashierName { get; set; }
        public decimal TotalAmount { get; set; }
        public string? PaymentMethod { get; set; }
        public string? Status { get; set; }    
        public string? Phone { get; set; }
        public string? CustomerName { get; set; }
        public DateTime SaleDate { get; set; }
        public TimeSpan SaleTime { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
