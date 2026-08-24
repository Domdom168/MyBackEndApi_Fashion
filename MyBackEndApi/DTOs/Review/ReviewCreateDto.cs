using System.ComponentModel.DataAnnotations;

namespace MyBackEndApi.DTOs.Review
{
    public class ReviewCreateDto
    {
        [Required]
        public int ProductId { get; set; }

        [Required]
        [Range(1, 5)]
        public int Rating { get; set; }

        [MaxLength(2000)]
        public string? Comment { get; set; }
    }
}
