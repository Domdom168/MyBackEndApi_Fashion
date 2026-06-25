using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyBackEndApi.Models
{
    [Table("orders")]
    public class Order
    {
        [Key]
        public int Id { get; set; }

        [Column("user_id")]
        public int? UserId { get; set; }
        //[ForeignKey(nameof(UserId))]
        public User? User { get; set; }

        [Column("cashier_id")]
        public int? CashierId { get; set; }
        //[ForeignKey(nameof(CashierId))]
        public Admin? Cashier { get; set; }

        [Column("total_amount")]
        public decimal TotalAmount { get; set; }
        [Column("status")]
        public string? Status { get; set; }

        [Column("customer_name")]
        public string? CustomerName { get; set; }
        [Column("phone")]
        public string? Phone { get; set; }
        [Column("address")]
        public string? Address { get; set; }

        [Column("payment_method")]
        public string? PaymentMethod { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; }
        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}
