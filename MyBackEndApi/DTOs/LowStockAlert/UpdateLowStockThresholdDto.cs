using System.ComponentModel.DataAnnotations;

namespace MyBackEndApi.DTOs.LowStockAlert
{
    public class UpdateLowStockThresholdDto
    {
        [Required]
        [Range(0, int.MaxValue)]
        public int Threshold { get; set; }
    }
}
