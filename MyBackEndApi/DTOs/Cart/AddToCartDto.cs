using System.ComponentModel.DataAnnotations;

namespace MyBackEndApi.DTOs.Cart
{
    public class AddToCartDto
    {

        [Required]
        public int ProductId { get; set; }
        [Required]
        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }
        public string? SelectedSize { get; set; }
        public string? SelectedColorName { get; set; }
        public string? SelectedColorHex { get; set; }
    }
}
