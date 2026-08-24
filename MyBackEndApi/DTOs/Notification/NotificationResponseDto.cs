namespace MyBackEndApi.DTOs.Notification
{
    public class NotificationResponseDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string? TitleKhmer { get; set; }
        public string? TitleEnglish { get; set; }
        public string? MessageKhmer { get; set; }
        public string? MessageEnglish { get; set; }
        public string? Type { get; set; }
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
