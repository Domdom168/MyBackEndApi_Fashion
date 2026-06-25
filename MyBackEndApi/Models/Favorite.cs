using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyBackEndApi.Models
{
    [Table("favorites")]
    public class Favorite
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Column("user_id")]
        public int UserId { get; set; }
        //[ForeignKey("id")]
        public User? User { get; set; }

        [Column("product_id")]
        public int ProductId { get; set; }
        //[ForeignKey("id")]
        public Product? Product { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }
    }
}
