using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyBackEndApi.DTOs.Notification;
using MyBackEndApi.Services;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
namespace MyBackEndApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // any logged-in user
    public class NotificationsController : ControllerBase
    {
        private readonly INotificationService _notificationService;

        public NotificationsController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        private int GetCurrentUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst(JwtRegisteredClaimNames.Sub);
            if (claim == null || !int.TryParse(claim.Value, out int userId))
                throw new UnauthorizedAccessException();
            return userId;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<NotificationResponseDto>>> GetNotifications()
        {
            var userId = GetCurrentUserId();
            var notifications = await _notificationService.GetNotificationsForUserAsync(userId);
            return Ok(notifications);
        }

        [HttpGet("unread-count")]
        public async Task<ActionResult<int>> GetUnreadCount()
        {
            var userId = GetCurrentUserId();
            var count = await _notificationService.GetUnreadCountAsync(userId);
            return Ok(count);
        }

        [HttpPut("{id}/read")]
        public async Task<IActionResult> MarkAsRead(int id, [FromBody] MarkReadDto dto)
        {
            var success = await _notificationService.MarkAsReadAsync(id, dto.IsRead);
            if (!success) return NotFound();
            return Ok(new { message = "Notification updated." });
        }

        [HttpPut("read-all")]
        public async Task<IActionResult> MarkAllAsRead()
        {
            var userId = GetCurrentUserId();
            await _notificationService.MarkAllAsReadAsync(userId);
            return Ok(new { message = "All notifications marked as read." });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteNotification(int id)
        {
            var deleted = await _notificationService.DeleteNotificationAsync(id);
            if (!deleted) return NotFound();
            return NoContent();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<NotificationResponseDto>> GetNotificationById(int id)
        {
            var userId = GetCurrentUserId();
            var notification = await _notificationService.GetNotificationByIdAsync(id);
            if (notification == null)
                return NotFound();
            if (notification.UserId != userId)
                return Forbid(); // not your notification
            return Ok(notification);
        }

        [HttpPost]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult<NotificationResponseDto>> CreateNotification([FromBody] CreateNotificationDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                var result = await _notificationService.CreateNotificationAsync(dto);
                // ✅ GetNotificationById exists in this controller (see above)
                return CreatedAtAction(nameof(GetNotificationById), new { id = result.Id }, result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}