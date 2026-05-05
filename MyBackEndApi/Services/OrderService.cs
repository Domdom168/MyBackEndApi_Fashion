using MyBackEndApi.Data;
using MyBackEndApi.DTOs.Order;
using MyBackEndApi.Models;
using Microsoft.EntityFrameworkCore;
namespace MyBackEndApi.Services
{
    public class OrderService: IOrderService
    {
        private readonly AppDbContext _context;
        private static readonly HashSet<string> ValidStatuses = new() { "pending", "processing", "completed", "cancelled" };
        private static readonly Dictionary<string, List<string>> AllowedTransitions = new()
        {
            ["pending"] = new() { "processing", "cancelled" },
            ["processing"] = new() { "completed", "cancelled" },
            ["completed"] = new(),
            ["cancelled"] = new()
        };

        public OrderService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<PagedResultDto<OrderResponseDto>> GetOrdersAsync(OrderFilterDto filter)
        {
            var query = _context.Orders
                .Include(o => o.User)
                .Include(o => o.Cashier)
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .AsQueryable();

            if (!string.IsNullOrEmpty(filter.Status) && ValidStatuses.Contains(filter.Status))
                query = query.Where(o => o.Status == filter.Status);
            if (!string.IsNullOrEmpty(filter.Search))
            {
                var search = filter.Search.ToLower();
                query = query.Where(o => o.CustomerName.ToLower().Contains(search) ||
                                         o.Phone.Contains(search) ||
                                         (o.User != null && o.User.Email.ToLower().Contains(search)));
            }

            var total = await query.CountAsync();
            var items = await query
                .OrderByDescending(o => o.CreatedAt)
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .Select(o => MapToResponseDto(o))
                .ToListAsync();

            return new PagedResultDto<OrderResponseDto>
            {
                Page = filter.Page,
                PageSize = filter.PageSize,
                TotalCount = total,
                Items = items
            };
        }

        public async Task<OrderResponseDto?> GetOrderByIdAsync(int id)
        {
            var order = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.Cashier)
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.Id == id);
            return order == null ? null : MapToResponseDto(order);
        }

        public async Task<bool> UpdateOrderStatusAsync(int id, string newStatus, int currentAdminId)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null) return false;

            var oldStatus = order.Status;
            if (!ValidStatuses.Contains(newStatus))
                throw new ArgumentException("Invalid status value");
            if (!AllowedTransitions[oldStatus].Contains(newStatus))
                throw new InvalidOperationException($"Cannot change from {oldStatus} to {newStatus}");

            order.Status = newStatus;
            order.UpdatedAt = DateTime.UtcNow;

            if (newStatus == "completed" && oldStatus != "completed")
            {
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
            return true;
        }

        public async Task<bool> DeleteOrderAsync(int id)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.Id == id);
            if (order == null) return false;

            _context.OrderItems.RemoveRange(order.OrderItems);
            _context.Orders.Remove(order);
            await _context.SaveChangesAsync();
            return true;
        }

        private static OrderResponseDto MapToResponseDto(Order order)
        {
            return new OrderResponseDto
            {
                Id = order.Id,
                UserId = order.UserId,
                CustomerName = order.CustomerName,
                Phone = order.Phone,
                Address = order.Address,
                TotalAmount = order.TotalAmount,
                Status = order.Status,
                PaymentMethod = order.PaymentMethod,
                CashierName = order.Cashier?.Name,
                CreatedAt = order.CreatedAt,
                Items = order.OrderItems.Select(oi => new OrderItemDto
                {
                    ProductId = oi.ProductId,
                    ProductName = oi.Product.NameEnglish ?? oi.Product.NameKhmer,
                    Quantity = oi.Quantity,
                    Price = oi.Price,
                    SelectedSize = oi.SelectedSize,
                    SelectedColorName = oi.SelectedColorName
                }).ToList()
            };
        }
    }
}
