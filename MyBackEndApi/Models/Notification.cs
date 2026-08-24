using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyBackEndApi.Models
{
    [Table("notifications")]
    public class Notification
    {
        [Key]
        public int Id { get; set; }
        [Column("user_id")]
        public int UserId { get; set; }
        public User? User { get; set; }

        [Column("title_khmer")]
        public string? TitleKhmer { get; set; }

        [Column("title_english")]
        public string? TitleEnglish { get; set; }

        [Column("message_khmer")]
        public string? MessageKhmer { get; set; }

        [Column("message_english")]
        public string? MessageEnglish { get; set; }

        [Column("type")]
        public string? Type { get; set; } = "info";

        [Column("is_read")]
        public bool IsRead { get; set; } = false;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }
    }
}
