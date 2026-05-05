using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyBackEndApi.Models
{
    [Table("sales")]
    public class Sale
    {
        [Key]
        public int Id { get; set; }

        [Column("order_id")]
        public int OrderId { get; set; }
        [ForeignKey("id")]
        public Order? Order { get; set; }

        [Column("cashier_id")]
        public int? CashierId { get; set; }
        [ForeignKey("id")]
        public Admin? Cashier { get; set; }

        [Column("total_amount")]
        public decimal TotalAmount { get; set; }

        [Column("payment_method")]
        public string? PaymentMethod { get; set; }
        
        [Column("sale_date")]
        public DateTime SaleDate { get; set; }
        [Column("sale_time")]
        public TimeSpan SaleTime { get; set; }
        [Column("created_at")]
        public DateTime CreatedAt { get; set; }
    }
}
