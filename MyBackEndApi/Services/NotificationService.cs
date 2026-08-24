using MyBackEndApi.Data;
using MyBackEndApi.DTOs.Notification;
using MyBackEndApi.Models;
using  Microsoft.EntityFrameworkCore;
namespace MyBackEndApi.Services
{
    public class NotificationService : INotificationService
    {
        private readonly AppDbContext _context;

        public NotificationService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<NotificationResponseDto>> GetNotificationsForUserAsync(int userId)
        {
            return await _context.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .Select(n => MapToDto(n))
                .ToListAsync();
        }

        public async Task<NotificationResponseDto?> GetNotificationByIdAsync(int id)
        {
            var notification = await _context.Notifications.FindAsync(id);
            return notification == null ? null : MapToDto(notification);
        }

        public async Task<NotificationResponseDto> CreateNotificationAsync(CreateNotificationDto dto)
        {
            var notification = new Notification
            {
                UserId = dto.UserId,
                TitleKhmer = dto.TitleKhmer,
                TitleEnglish = dto.TitleEnglish,
                MessageKhmer = dto.MessageKhmer,
                MessageEnglish = dto.MessageEnglish,
                Type = dto.Type ?? "info",
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };
            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();
            return MapToDto(notification);
        }

        public async Task<bool> MarkAsReadAsync(int id, bool isRead)
        {
            var notification = await _context.Notifications.FindAsync(id);
            if (notification == null) return false;
            notification.IsRead = isRead;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> MarkAllAsReadAsync(int userId)
        {
            var notifications = await _context.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .ToListAsync();
            foreach (var n in notifications)
            {
                n.IsRead = true;
            }
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteNotificationAsync(int id)
        {
            var notification = await _context.Notifications.FindAsync(id);
            if (notification == null) return false;
            _context.Notifications.Remove(notification);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<int> GetUnreadCountAsync(int userId)
        {
            return await _context.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .CountAsync();
        }

        private static NotificationResponseDto MapToDto(Notification n) => new()
        {
            Id = n.Id,
            UserId = n.UserId,
            TitleKhmer = n.TitleKhmer,
            TitleEnglish = n.TitleEnglish,
            MessageKhmer = n.MessageKhmer,
            MessageEnglish = n.MessageEnglish,
            Type = n.Type,
            IsRead = n.IsRead,
            CreatedAt = n.CreatedAt
        };
    }
}
