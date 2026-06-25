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
    [Authorize]
    public class CustomerOrdersController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IOrderService _orderService;

        public CustomerOrdersController(AppDbContext context, IOrderService orderService)
        {
            _context = context;
            _orderService = orderService;

        }

        private int GetUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst(JwtRegisteredClaimNames.Sub);
            if (claim == null) throw new UnauthorizedAccessException();
            var userId = int.Parse(claim.Value);
            Console.WriteLine($"Extracted user ID: {userId}"); // check console
            return userId;
        }

        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderDto dto)
        {
            var userId = GetUserId();
            // Verify that the user exists in the users table
            var user = await _context.Users.FindAsync(userId);
            Console.WriteLine($"Received order for: {dto.CustomerName}, Items: {dto.Items?.Count}");
            if (user == null)
                return BadRequest(new { message = "User not found. Please log in as a customer." });
            var order = new Order
            {
                UserId = userId,
                CustomerName = dto.CustomerName,
                Phone = dto.Phone,
                Address = dto.Address,
                TotalAmount = dto.Items.Sum(i => i.Price * i.Quantity),
                Status = "pending",
                PaymentMethod = dto.PaymentMethod,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                OrderItems = dto.Items.Select(i => new OrderItem
                {
                    ProductId = i.ProductId,
                    Quantity = i.Quantity,
                    Price = i.Price,
                    SelectedSize = i.SelectedSize,
                    SelectedColorName = i.SelectedColorName
                }).ToList()
            };

            // Deduct stock
            foreach (var item in dto.Items)
            {
                var product = await _context.Products.FindAsync(item.ProductId);
                if (product == null) return BadRequest($"Product {item.ProductId} not found");
                if (product.Stock < item.Quantity)
                    return BadRequest($"Insufficient stock for product {product.NameEnglish}");
                product.Stock -= item.Quantity;
            }

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            // Optionally clear cart
            var cartItems = await _context.Carts.Where(c => c.UserId == userId).ToListAsync();
            _context.Carts.RemoveRange(cartItems);
            await _context.SaveChangesAsync();

            return Ok(new { orderId = order.Id, message = "Order placed successfully" });
        }

        // GET api/customer/orders
        [HttpGet]
        public async Task<IActionResult> GetMyOrders()
        {
            var userId = GetUserId();
            var orders = await _context.Orders
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.CreatedAt)
                .Select(o => new
                {
                    o.Id,
                    o.CustomerName,
                    o.Phone,
                    o.Address,
                    o.TotalAmount,
                    o.Status,
                    o.PaymentMethod,
                    o.CreatedAt,
                    Items = o.OrderItems.Select(oi => new
                    {
                        oi.ProductId,
                        ProductName = oi.Product.NameEnglish ?? oi.Product.NameKhmer,
                        oi.Quantity,
                        oi.Price,
                        oi.SelectedSize,
                        oi.SelectedColorName
                    })
                })
                .ToListAsync();
            return Ok(orders);
        }

        // GET api/customer/orders/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrderById(int id)
        {
            var userId = GetUserId();
            var order = await _context.Orders
                .Where(o => o.Id == id && o.UserId == userId)
                .Select(o => new
                {
                    o.Id,
                    o.CustomerName,
                    o.Phone,
                    o.Address,
                    o.TotalAmount,
                    o.Status,
                    o.PaymentMethod,
                    o.CreatedAt,
                    Items = o.OrderItems.Select(oi => new
                    {
                        oi.ProductId,
                        ProductName = oi.Product.NameEnglish ?? oi.Product.NameKhmer,
                        oi.Quantity,
                        oi.Price,
                        oi.SelectedSize,
                        oi.SelectedColorName
                    })
                })
                .FirstOrDefaultAsync();
            if (order == null) return NotFound();
            return Ok(order);
        }
        [HttpPut("{id}/status")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> UpdateOrderStatus(int id, [FromBody] OrderUpdateStatusDto dto)
        {
            var currentAdminId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var success = await _orderService.UpdateOrderStatusAsync(id, dto.Status, currentAdminId);
            if (!success) return NotFound();
            return Ok(new { message = "Order status updated" });
        }
    }
}
