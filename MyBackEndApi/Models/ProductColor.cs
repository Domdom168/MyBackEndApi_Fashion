using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyBackEndApi.Models
{
    [Table("product_colors")]
    public class ProductColor
    {
        [Key]
        public int Id { get; set; }

        [Column("product_id")]
        public int product_id { get; set; }
        public Product? Product { get; set; }

        [Column("color_name")]
        public string? ColorName { get; set; }

        [Column("color_hex")]
        public string? ColorHex { get; set; } 
    }
}
