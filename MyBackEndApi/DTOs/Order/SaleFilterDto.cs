namespace MyBackEndApi.DTOs.Order
{
    public class SaleFilterDto
    {
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string? PaymentMethod { get; set; }
        public int? CashierId { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}
