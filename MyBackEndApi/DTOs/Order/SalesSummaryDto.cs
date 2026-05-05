namespace MyBackEndApi.DTOs.Order
{
    public class SalesSummaryDto
    {
        public decimal TotalRevenue { get; set; }
        public int TotalOrders { get; set; }
        public decimal AverageOrderValue { get; set; }
        public Dictionary<string, decimal> PaymentMethodBreakdown { get; set; } = new();
        public List<DailySalesDto> DailySales { get; set; } = new();
    }

    public class DailySalesDto
    {
        public DateTime Date { get; set; }
        public decimal Revenue { get; set; }
        public int Orders { get; set; }
    }
}
