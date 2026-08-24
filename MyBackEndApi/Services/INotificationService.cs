using MyBackEndApi.DTOs.Notification;

namespace MyBackEndApi.Services
{
    public interface INotificationService
    {
        Task<IEnumerable<NotificationResponseDto>> GetNotificationsForUserAsync(int userId);
        Task<NotificationResponseDto?> GetNotificationByIdAsync(int id);
        Task<NotificationResponseDto> CreateNotificationAsync(CreateNotificationDto dto);
        Task<bool> MarkAsReadAsync(int id, bool isRead);
        Task<bool> MarkAllAsReadAsync(int userId);
        Task<bool> DeleteNotificationAsync(int id);
        Task<int> GetUnreadCountAsync(int userId);
    }
}
