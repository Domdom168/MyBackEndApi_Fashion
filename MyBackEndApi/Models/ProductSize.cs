using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyBackEndApi.Models
{
    [Table("product_sizes")]
    public class ProductSize
    {
        [Key]
        public int Id { get; set; }

        [Column("product_id")]
        public int ProductId { get; set; }
        public Product? Product { get; set; }

        [Column("size")]
        public string? Size { get; set; }
 
    }
}
