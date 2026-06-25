namespace MyBackEndApi.DTOs.Favorite
{
    public class FavoriteResponseDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string? ProductName { get; set; }
        public string? ProductImage { get; set; }
        public decimal Price { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
