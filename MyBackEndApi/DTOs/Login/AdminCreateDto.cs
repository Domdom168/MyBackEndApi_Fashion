using System.ComponentModel.DataAnnotations;

namespace MyBackEndApi.DTOs.Login
{
    public class AdminCreateDto
    {
        [Required]
        public string? Name { get; set; }

        [Required]
        [EmailAddress]
        public string? Email { get; set; }

        [Required]
        [MinLength(6)]
        public string? Password { get; set; }

        public string? Phone { get; set; }

        [Required]
        public string? Role { get; set; } // "admin" or "cashier"

        public bool IsActive { get; set; } = true;
    }
}
