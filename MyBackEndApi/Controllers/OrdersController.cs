using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyBackEndApi.Data;
using MyBackEndApi.DTOs.Order;
using MyBackEndApi.Models;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
namespace MyBackEndApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "admin,cashier")] // both roles can access
    public class OrdersController : ControllerBase
    {
        private readonly AppDbContext _context;

        public OrdersController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/orders
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetAllOrders()
        {
            var orders = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.Cashier)
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
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
                    CashierName = o.Cashier != null ? o.Cashier.Name : null,
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

        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderDto dto)
        {
            // Get current user ID from token (could be admin or customer)
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst(JwtRegisteredClaimNames.Sub);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int currentUserId))
                return Unauthorized();

            // Check if user exists in users table (customer)
            var user = await _context.Users.FindAsync(currentUserId);
            int? orderUserId = user != null ? currentUserId : (int?)null;

            // Get cashier/admin ID (the one who is creating the order)
            int cashierId = currentUserId; // the token belongs to admin/cashier or customer
                                           // But if it's a customer, we don't want to set CashierId? Actually we can set it regardless.
                                           // For customer orders from the storefront, CashierId can be null.

            // Optionally, if the user is not a customer, we should set CashierId
            int? cashierIdToStore = user == null ? currentUserId : (int?)null;

            // Validate items
            decimal total = 0;
            var orderItems = new List<OrderItem>();
            foreach (var itemDto in dto.Items)
            {
                var product = await _context.Products.FindAsync(itemDto.ProductId);
                if (product == null) return BadRequest($"Product {itemDto.ProductId} not found");
                if (product.Stock < itemDto.Quantity)
                    return BadRequest($"Insufficient stock for product {product.NameEnglish}");

                total += itemDto.Price * itemDto.Quantity;
                orderItems.Add(new OrderItem
                {
                    ProductId = itemDto.ProductId,
                    Quantity = itemDto.Quantity,
                    Price = itemDto.Price,
                    SelectedSize = itemDto.SelectedSize,
                    SelectedColorName = itemDto.SelectedColorName
                });
                product.Stock -= itemDto.Quantity;
            }

            var order = new Order
            {
                UserId = orderUserId,
                CashierId = cashierIdToStore, // if admin/cashier, store their ID
                CustomerName = dto.CustomerName,
                Phone = dto.Phone,
                Address = dto.Address,
                TotalAmount = total,
                Status = "pending",
                PaymentMethod = dto.PaymentMethod,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                OrderItems = orderItems
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            // Clear cart if there is a user
            if (orderUserId.HasValue)
            {
                var cartItems = await _context.Carts.Where(c => c.UserId == orderUserId.Value).ToListAsync();
                if (cartItems.Any())
                {
                    _context.Carts.RemoveRange(cartItems);
                    await _context.SaveChangesAsync();
                }
            }

            return Ok(new { id = order.Id, totalAmount = order.TotalAmount });
        }
        // PUT: api/orders/{id}/status
        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateOrderStatus(int id, [FromBody] OrderUpdateStatusDto dto)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null) return NotFound();

            var allowedTransitions = new Dictionary<string, List<string>>
            {
                ["pending"] = new() { "processing", "cancelled", "completed" },
                ["processing"] = new() { "completed", "cancelled" },
                ["completed"] = new(),
                ["cancelled"] = new()
            };

            if (!allowedTransitions[order.Status].Contains(dto.Status))
                return Conflict(new { message = $"Cannot change status from {order.Status} to {dto.Status}" });

            // Capture old status before change
            var oldStatus = order.Status;

            // Update order status
            order.Status = dto.Status;
            order.UpdatedAt = DateTime.UtcNow;

            // If order becomes completed (from a non-completed status), create a sales record
            if (dto.Status == "completed" && oldStatus != "completed")
            {
                var currentAdminId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
                var sale = new Sale
                {
                    OrderId = order.Id,
                    CashierId = currentAdminId,
                    TotalAmount = order.TotalAmount,
                    PaymentMethod = order.PaymentMethod,
                    SaleDate = DateTime.UtcNow,
                    SaleTime = DateTime.UtcNow.TimeOfDay,
                    CreatedAt = DateTime.UtcNow
                };
                _context.Sales.Add(sale);
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = "Order status updated" });
        }

        // DELETE: api/orders/{id}
        [HttpDelete("{id}")]
        [Authorize(Roles = "admin")] // only admin can delete
        public async Task<IActionResult> DeleteOrder(int id)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.Id == id);
            if (order == null) return NotFound();

            _context.OrderItems.RemoveRange(order.OrderItems);
            _context.Orders.Remove(order);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Order deleted" });
        }
    }
}
