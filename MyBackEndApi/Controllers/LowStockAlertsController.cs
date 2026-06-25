using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyBackEndApi.DTOs.LowStockAlert;
using MyBackEndApi.Services;

namespace MyBackEndApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "admin")]
    public class LowStockAlertsController : ControllerBase
    {
        private readonly ILowStockAlertService _alertService;

        public LowStockAlertsController(ILowStockAlertService alertService)
        {
            _alertService = alertService;
        }

        [HttpGet]
        public async Task<ActionResult<List<LowStockAlertDto>>> GetAllAlerts()
        {
            var alerts = await _alertService.GetAllAlertsAsync();
            return Ok(alerts);
        }

        [HttpGet("product/{productId}")]
        public async Task<ActionResult<LowStockAlertDto>> GetAlertByProduct(int productId)
        {
            var alert = await _alertService.GetAlertByProductIdAsync(productId);
            if (alert == null) return NotFound();
            return Ok(alert);
        }

        [HttpPost("product/{productId}/threshold")]
        public async Task<ActionResult<LowStockAlertDto>> SetThreshold(int productId, [FromBody] UpdateLowStockThresholdDto dto)
        {
            try
            {
                var alert = await _alertService.CreateOrUpdateAlertAsync(productId, dto.Threshold);
                return Ok(alert);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAlert(int id)
        {
            var deleted = await _alertService.DeleteAlertAsync(id);
            if (!deleted) return NotFound();
            return Ok(new { message = "Alert deleted" });
        }
    }
}
