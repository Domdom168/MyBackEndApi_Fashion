using System.ComponentModel.DataAnnotations;

namespace MyBackEndApi.DTOs.Color
{
    public class UpdateProductColorDto
    {
        [Required]
        [MaxLength(50)]
        public string? ColorName { get; set; }

        [Required]
        [RegularExpression("^#([A-Fa-f0-9]{6}|[A-Fa-f0-9]{3})$", ErrorMessage = "Invalid hex color format")]
        public string? ColorHex { get; set; }
    }

}
