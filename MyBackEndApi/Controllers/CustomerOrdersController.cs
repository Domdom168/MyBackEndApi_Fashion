using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyBackEndApi.Data;
using MyBackEndApi.DTOs.Order;
using MyBackEndApi.Models;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.EntityFrameworkCore;
using MyBackEndApi.Services;
namespace MyBackEndApi.Controllers
{
    [Route("api/customer/orders")]
    [ApiController]
    [Authorize] // requires customer login
    public class CustomerOrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public CustomerOrdersController(IOrderService orderService)
        {
            _orderService = orderService;
        }
        [HttpPost]
        public async Task<ActionResult<OrderResponseDto>> CreateOrder([FromBody] OrderCreateDto dto)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                if (userId == 0) return Unauthorized();

                var order = await _orderService.CreateOrderAsync(dto, userId, null);
                var orderDto = await _orderService.GetOrderByIdAsync(order.Id);
                return Ok(new { orderId = orderDto.Id, totalAmount = orderDto.TotalAmount });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<OrderResponseDto>>> GetMyOrders()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var filter = new OrderFilterDto { Page = 1, PageSize = 100 };
            var result = await _orderService.GetOrdersAsync(filter);
            var myOrders = result.Items.Where(o => o.UserId == userId);
            return Ok(myOrders);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<OrderResponseDto>> GetOrderById(int id)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var order = await _orderService.GetOrderByIdAsync(id);
            if (order == null || order.UserId != userId)
                return NotFound();
            return Ok(order);
        }
    }
}
