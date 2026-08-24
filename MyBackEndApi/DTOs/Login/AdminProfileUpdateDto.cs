using System.ComponentModel.DataAnnotations;

namespace MyBackEndApi.DTOs.Login
{
    public class AdminProfileUpdateDto
    {
        public string? Name { get; set; }

        [EmailAddress]
        public string? Email { get; set; }

        public string? Phone { get; set; }
    }
}
