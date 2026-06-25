using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyBackEndApi.Models
{
    [Table("activity_logs")]
    public class ActivityLog
    {
        [Key]
        public int Id { get; set; }
        [Column("user_id")]
        public int? UserId { get; set; }
        public User? User { get; set; }

        [Column("user_type")]
        public string? UserType { get; set; } // "admin", "cashier", "user"
        [Column("action")]
        public string? Action { get; set; }
        [Column("description")]
        public string? Description { get; set; }

        [Column("ip_address")]
        public string? IpAddress { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }
    }
}
