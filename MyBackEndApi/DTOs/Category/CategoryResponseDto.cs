namespace MyBackEndApi.DTOs.Category
{
    public class CategoryResponseDto
    {
        public int Id { get; set; }
        public string? NameKhmer { get; set; }
        public string? NameEnglish { get; set; }
        public string? Icon { get; set; }
        public string? Color { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
