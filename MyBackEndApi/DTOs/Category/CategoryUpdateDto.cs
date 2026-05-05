using System.ComponentModel.DataAnnotations;

namespace MyBackEndApi.DTOs.Category
{
    public class CategoryUpdateDto
    {
        [Required]
        public string? NameKhmer { get; set; }

        [Required]
        public string? NameEnglish { get; set; }
        public string? Icon { get; set; }
        public string? Color { get; set; }
    }
}
