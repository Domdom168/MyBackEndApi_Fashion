using System.ComponentModel.DataAnnotations;

namespace MyBackEndApi.DTOs.Login
{
    public class AdminResetPasswordDto
    {
        [Required]
        [MinLength(6)]
        public string? NewPassword { get; set; }
    }
}
