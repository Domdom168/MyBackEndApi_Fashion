using System.ComponentModel.DataAnnotations;

namespace MyBackEndApi.DTOs.Size
{
    public class CreateProductSizeDto
    {
        [Required]
        public int ProductId { get; set; }

        [Required]
        [MaxLength(10)]
        public string? Size { get; set; }
    }
}
