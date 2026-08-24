using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyBackEndApi.Models
{
    [Table("admins")]
    public class Admin
    {
        [Key]
        public int Id { get; set; }
        [Column("name")]
        public string? Name { get; set; }
        [Column("email")]
        public string? Email { get; set; }
        [Column("password")]
        public string? Password { get; set; }
        [Column("phone")]
        public string? Phone { get; set; }
        [Column("role")]
        public string? Role { get; set; }
        [Column("is_active")]
        public bool IsActive { get; set; } = true;
        [Column("created_at")]
        public DateTime CreatedAt { get; set; }
        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; }
        public string? ResetToken { get; set; }
        public DateTime? ResetTokenExpiry { get; set; }
    }
}
