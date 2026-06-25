using MyBackEndApi.DTOs.ActivityLog;
using MyBackEndApi.DTOs.Order;

namespace MyBackEndApi.Services
{
    public interface IActivityLogService
    {
        Task<PagedResultDto<ActivityLogDto>> GetLogsAsync(ActivityLogFilterDto filter);
        Task LogAsync(int? userId, string userType, string action, string description, string ipAddress);
    }
}
