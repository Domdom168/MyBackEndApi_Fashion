using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyBackEndApi.Models
{
    [Table("password_reset_tokens")]
    public class PasswordResetToken
    {
        [Key]
        public int Id { get; set; }
        public string? Email { get; set; }
        public string? Token { get; set; }
        [Column("expires_at")]
        public DateTime ExpiresAt { get; set; }
        [Column("created_at")]
        public DateTime CreatedAt { get; set; }
    }
}
