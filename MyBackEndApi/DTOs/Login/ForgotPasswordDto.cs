using System.ComponentModel.DataAnnotations;

namespace MyBackEndApi.DTOs.Login
{
    public class ForgotPasswordDto
    {
        [Required]
        [EmailAddress]
        public string? Email { get; set; }
    }
    //public class VerifyCodeDto
    //{
    //    [EmailAddress]
    //    public string? Email { get; set; }
    //    public string? Code { get; set; }
    //}
}
