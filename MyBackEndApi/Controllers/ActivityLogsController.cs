using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyBackEndApi.DTOs.ActivityLog;
using MyBackEndApi.DTOs.Order;
using MyBackEndApi.Services;

namespace MyBackEndApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "admin")]  // only admins can view logs
    public class ActivityLogsController : ControllerBase
    {
        private readonly IActivityLogService _logService;

        public ActivityLogsController(IActivityLogService logService)
        {
            _logService = logService;
        }

        [HttpGet]
        public async Task<ActionResult<PagedResultDto<ActivityLogDto>>> GetLogs([FromQuery] ActivityLogFilterDto filter)
        {
            var result = await _logService.GetLogsAsync(filter);
            return Ok(result);
        }
    }
}
