using Microsoft.AspNetCore.Mvc;
using MyBackEndApi.Data;
using MyBackEndApi.DTOs;
using MyBackEndApi.Services;
using Microsoft.EntityFrameworkCore;
namespace MyBackEndApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IBakongService _bakongService;

        public PaymentController(AppDbContext context, IBakongService bakongService)
        {
            _context = context;
            _bakongService = bakongService;
        }

        [HttpPost("generate-qr")]
        public async Task<ActionResult<QRResponseDto>> GenerateQR([FromBody] GenerateQRRequestDto dto)
        {
            var order = await _context.Orders.FindAsync(dto.OrderId);
            if (order == null) return NotFound();

            var billNumber = $"ORD-{order.Id}-{DateTime.UtcNow.Ticks}";
            var qrString = await _bakongService.GenerateDynamicQRAsync(order.TotalAmount, billNumber);

            // Optionally save billNumber to order
            order.PaymentMethod= billNumber;
            await _context.SaveChangesAsync();

            return Ok(new QRResponseDto { QrString = qrString, BillNumber = billNumber });
        }
    }
}
