namespace MyBackEndApi.DTOs.LowStockAlert
{
    public class LowStockAlertDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string? ProductName { get; set; }
        public int CurrentStock { get; set; }
        public int Threshold { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
