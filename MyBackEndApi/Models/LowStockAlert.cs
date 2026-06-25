using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace MyBackEndApi.Models
{
    [Table("low_stock_alerts")]
    public class LowStockAlert
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [Column("product_id")]
        public int ProductId { get; set; }
        public Product? Product { get; set; }
        [Column("threshold")]
        public int Threshold { get; set; }
        [Column("is_active")]
        public bool IsActive { get; set; }
        [Column("created_at")]
        public DateTime CreatedAt { get; set; }
    }
}
