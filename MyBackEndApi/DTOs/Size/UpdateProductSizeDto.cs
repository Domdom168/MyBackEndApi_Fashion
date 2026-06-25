using System.ComponentModel.DataAnnotations;

namespace MyBackEndApi.DTOs.Size
{
    public class UpdateProductSizeDto
    {
        [Required]
        [MaxLength(10)]
        public string? Size { get; set; }
    }
}
