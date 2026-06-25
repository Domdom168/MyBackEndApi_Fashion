using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyBackEndApi.Models
{
    [Table("carts")]
    public class Cart
    {
        [Key]
        public int Id { get; set; }

        //[ForeignKey("User")]
        [Column("user_id")]
        public int UserId { get; set; }
        public User? User { get; set; }

        //[ForeignKey("Product")]
        [Column("product_id")]
        public int ProductId { get; set; }
        public Product? Product { get; set; }
        [Column("quantity")]
        public int Quantity { get; set; }

        [Column("selected_size")]
        public string? SelectedSize { get; set; }

        [Column("selected_color_name")]
        public string? SelectedColorName { get; set; }

        [Column("selected_color_hex")]
        public string? SelectedColorHex { get; set; }
        [Column("created_at")]
        public DateTime CreatedAt { get; set; }
    }
}
