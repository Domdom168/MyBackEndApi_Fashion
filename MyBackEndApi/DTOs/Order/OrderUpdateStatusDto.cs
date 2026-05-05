using System.ComponentModel.DataAnnotations;

namespace MyBackEndApi.DTOs.Order
{
    public class OrderUpdateStatusDto
    {
        [Required]
        [RegularExpression("^(pending|processing|completed|cancelled)$",
           ErrorMessage = "Status must be pending, processing, completed, or cancelled")]
        public string? Status { get; set; }
    }
}
