using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyBackEndApi.Models
{
    [Table("banners")]
    public class Banner
    {
        [Key]
        public int Id { get; set; }
        [Column("title")]
        public string? Title { get; set; }

        [Column("image_url")]
        public string? ImageUrl { get; set; }

        public string? Link { get; set; }

        [Column("display_order")]
        public int DisplayOrder { get; set; }

        [Column("is_active")]
        public bool IsActive { get; set; } = true;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; }
    }
}
