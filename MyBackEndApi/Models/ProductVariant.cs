using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyBackEndApi.Models
{
    [Table("product_variants")]
    public class ProductVariant
    {
        [Key]
        public int Id { get; set; }

        //[ForeignKey("Product")]
        [Column("product_id")]
        public int ProductId { get; set; }
        public Product? Product { get; set; }

        [Column("size")]
        public string? Size { get; set; }

        [Column("color_name")]
        public string? ColorName { get; set; }

        [Column("color_hex")]
        public string? ColorHex { get; set; }

        [Column("stock")]
        public int Stock { get; set; }

        [Column("price")]
        public decimal? Price { get; set; } // optional variant price

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; }
    }
}