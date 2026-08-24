using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyBackEndApi.Models
{
    [Table("product_images")]
    public class ProductImage
    {
        [Key]
        public int Id { get; set; }

        [Column("product_id")]
        public int product_id { get; set; }
        public Product? Product { get; set; }

        [Column("image_url")]
        public string? ImageUrl { get; set; }

        [Column("display_order")]
        public int DisplayOrder { get; set; }
    }
}
