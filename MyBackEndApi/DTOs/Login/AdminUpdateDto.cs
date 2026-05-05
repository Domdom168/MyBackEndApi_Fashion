using System.ComponentModel.DataAnnotations;

namespace MyBackEndApi.DTOs.Login
{
    public class AdminUpdateDto
    {
        [Required]
        public string? Name { get; set; }

        [Required]
        [EmailAddress]
        public string? Email { get; set; }

        public string? Phone { get; set; }

        [Required]
        public string? Role { get; set; } // "admin" or "cashier"

        public bool IsActive { get; set; }
    }
}
