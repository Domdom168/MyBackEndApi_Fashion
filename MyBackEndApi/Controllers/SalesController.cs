using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyBackEndApi.DTOs.Order;
using MyBackEndApi.Services;

namespace MyBackEndApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "admin,cashier")]
    public class SalesController : ControllerBase
    {
        private readonly ISalesService _salesService;

        public SalesController(ISalesService salesService)
        {
            _salesService = salesService;
        }

        [HttpGet]
        public async Task<ActionResult<PagedResultDto<SaleResponseDto>>> GetSales([FromQuery] SaleFilterDto filter)
        {
            var result = await _salesService.GetSalesAsync(filter);
            return Ok(result);
        }

        [HttpGet("summary")]
        public async Task<ActionResult<SalesSummaryDto>> GetSalesSummary([FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate)
        {
            var summary = await _salesService.GetSalesSummaryAsync(fromDate, toDate);
            return Ok(summary);
        }
    }
}
