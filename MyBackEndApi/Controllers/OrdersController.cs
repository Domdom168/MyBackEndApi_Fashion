using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyBackEndApi.Data;
using MyBackEndApi.DTOs.Order;
using MyBackEndApi.Models;
using MyBackEndApi.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
namespace MyBackEndApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "admin,cashier")]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrdersController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        [HttpGet]
        public async Task<ActionResult<PagedResultDto<OrderResponseDto>>> GetOrders([FromQuery] OrderFilterDto filter)
        {
            return Ok(await _orderService.GetOrdersAsync(filter));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<OrderResponseDto>> GetOrderById(int id)
        {
            var order = await _orderService.GetOrderByIdAsync(id);
            if (order == null) return NotFound();
            return Ok(order);
        }
        [HttpPost]
        public async Task<ActionResult<OrderResponseDto>> CreateOrder([FromBody] OrderCreateDto dto)
        {
            try
            {
                var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                var user = await _orderService.GetUserByIdAsync(currentUserId);
                int? orderUserId = user != null ? currentUserId : (int?)null;
                int? cashierId = user == null ? currentUserId : (int?)null;

                var order = await _orderService.CreateOrderAsync(dto, orderUserId, cashierId);
                var orderDto = await _orderService.GetOrderByIdAsync(order.Id); // returns DTO
                return CreatedAtAction(nameof(GetOrderById), new { id = orderDto.Id }, orderDto);
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


        [HttpPut("{id}/status")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] OrderUpdateStatusDto dto)
        {
            try
            {
                var currentAdminId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                var success = await _orderService.UpdateOrderStatusAsync(id, dto.Status, currentAdminId);
                if (!success) return NotFound();
                return Ok(new { message = "Order status updated" });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> DeleteOrder(int id)
        {
            var deleted = await _orderService.DeleteOrderAsync(id);
            if (!deleted) return NotFound();
            return NoContent();
        }
    }
}
