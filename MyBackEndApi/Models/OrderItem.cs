using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyBackEndApi.Models
{
    [Table("order_items")]
    public class OrderItem
    {
        [Key]
        public int Id { get; set; }

        [Column("order_id")]
        public int OrderId { get; set; }
        //[ForeignKey("id")]
        public Order? Order { get; set; }

        [Column("product_id")]
        public int ProductId { get; set; }
        //[ForeignKey("id")]
        public Product? Product { get; set; }
        [Column("quantity")]
        public int Quantity { get; set; }

        [Column("price")]
        public decimal Price { get; set; }
        [Column("selected_size")]
        public string? SelectedSize { get; set; }
        [Column("selected_color_name")]
        public string? SelectedColorName { get; set; }
    }
}
