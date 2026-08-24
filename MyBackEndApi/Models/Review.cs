using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyBackEndApi.Models
{
    [Table("reviews")]
    public class Review
    {
        [Key]
        public int Id { get; set; }

        //[ForeignKey("Product")]
        [Column("product_id")]
        public int ProductId { get; set; }
        public Product? Product { get; set; }

        //[ForeignKey("User")]
        [Column("user_id")]
        public int UserId { get; set; }
        public User? User { get; set; }
        [Column("rating")]

        public int Rating { get; set; } // 1-5

        public string? Comment { get; set; }

        [Column("is_approved")]
        public bool IsApproved { get; set; } = false;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; }
    }
}