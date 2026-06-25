namespace MyBackEndApi.DTOs.ActivityLog
{
    public class ActivityLogDto
    {
        public int Id { get; set; }
        public int? UserId { get; set; }
        public string? UserName { get; set; }
        public string? UserType { get; set; }
        public string? Action { get; set; }
        public string? Description { get; set; }
        public string? IpAddress { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
