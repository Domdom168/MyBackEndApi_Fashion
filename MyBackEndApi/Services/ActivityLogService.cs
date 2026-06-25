using MyBackEndApi.Data;
using MyBackEndApi.DTOs.ActivityLog;
using MyBackEndApi.DTOs.Order;
using MyBackEndApi.Models;
using Microsoft.EntityFrameworkCore;
namespace MyBackEndApi.Services
{
    public class ActivityLogService: IActivityLogService
    {
        private readonly AppDbContext _context;

        public ActivityLogService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<PagedResultDto<ActivityLogDto>> GetLogsAsync(ActivityLogFilterDto filter)
        {
            var query = _context.ActivityLogs
                .Include(l => l.User)
                .AsQueryable();

            if (!string.IsNullOrEmpty(filter.Action))
                query = query.Where(l => l.Action == filter.Action);
            if (!string.IsNullOrEmpty(filter.UserType))
                query = query.Where(l => l.UserType == filter.UserType);
            if (filter.UserId.HasValue)
                query = query.Where(l => l.UserId == filter.UserId);
            if (filter.FromDate.HasValue)
                query = query.Where(l => l.CreatedAt >= filter.FromDate.Value);
            if (filter.ToDate.HasValue)
                query = query.Where(l => l.CreatedAt <= filter.ToDate.Value);

            var total = await query.CountAsync();
            var items = await query
                .OrderByDescending(l => l.CreatedAt)
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .Select(l => new ActivityLogDto
                {
                    Id = l.Id,
                    UserId = l.UserId,
                    UserName = l.User != null ? l.User.Name : null,
                    UserType = l.UserType,
                    Action = l.Action,
                    Description = l.Description,
                    IpAddress = l.IpAddress,
                    CreatedAt = l.CreatedAt
                })
                .ToListAsync();

            return new PagedResultDto<ActivityLogDto>
            {
                Page = filter.Page,
                PageSize = filter.PageSize,
                TotalCount = total,
                Items = items
            };
        }

        public async Task LogAsync(int? userId, string userType, string action, string description, string ipAddress)
        {
            var log = new ActivityLog
            {
                UserId = userId,
                UserType = userType,
                Action = action,
                Description = description,
                IpAddress = ipAddress,
                CreatedAt = DateTime.UtcNow
            };
            _context.ActivityLogs.Add(log);
            await _context.SaveChangesAsync();
        }
    }
}
