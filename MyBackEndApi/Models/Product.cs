using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyBackEndApi.Models
{
    [Table("products")]
    public class Product
    {
        [Key]
        public int Id { get; set; }

        [Column("category_id")]
        public int category_id { get; set; }
        public Category? Category { get; set; }

        [Column("name_khmer")]
        public string? NameKhmer { get; set; }

        [Column("name_english")]
        public string? NameEnglish { get; set; }

        [Column("description_khmer")]
        public string DescriptionKhmer { get; set; } = "";

        [Column("description_english")]
        public string DescriptionEnglish { get; set; } = "";

        [Column(TypeName = "decimal(10,2)")]
        public decimal Price { get; set; }

        public int Stock { get; set; }

        [Column(TypeName = "decimal(3,2)")]
        public decimal Rating { get; set; }

        public int Reviews { get; set; }

        [Column("image_url")]
        public string? ImageUrl { get; set; }   // primary image

        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }

        public ICollection<ProductImage> ProductImages { get; set; } = new List<ProductImage>();
        public ICollection<ProductColor> ProductColors { get; set; } = new List<ProductColor>();
        public ICollection<ProductSize> ProductSizes { get; set; } = new List<ProductSize>();

    }
}
