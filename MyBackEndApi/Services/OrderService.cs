using Microsoft.EntityFrameworkCore;
using MyBackEndApi.Data;
using MyBackEndApi.DTOs.Order;
using MyBackEndApi.Models;
using System.Transactions;

namespace MyBackEndApi.Services
{
    public class OrderService : IOrderService
    {
        private readonly AppDbContext _context;
        private static readonly HashSet<string> ValidStatuses = new() { "pending", "processing", "completed", "cancelled" };
        private static readonly Dictionary<string, List<string>> AllowedTransitions = new()
        {
            ["pending"] = new() { "processing", "cancelled", "completed" },
            ["processing"] = new() { "completed", "cancelled" },
            ["completed"] = new(),
            ["cancelled"] = new()
        };

        public OrderService(AppDbContext context)
        {
            _context = context;
        }

        private async Task UpdateProductTotalStockAsync(int productId)
        {
            var totalStock = await _context.ProductVariants
                .Where(v => v.ProductId == productId)
                .SumAsync(v => v.Stock);
            var product = await _context.Products.FindAsync(productId);
            if (product != null)
            {
                product.Stock = totalStock;
                await _context.SaveChangesAsync();
            }
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

        public async Task<Order> CreateOrderAsync(OrderCreateDto dto, int? userId, int? cashierId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Validate required fields
                if (string.IsNullOrWhiteSpace(dto.CustomerName))
                    throw new ArgumentException("Customer name is required.");
                if (dto.Items == null || !dto.Items.Any())
                    throw new ArgumentException("Order must contain at least one item.");
                var order = new Order
                {
                    UserId = userId,
                    CashierId = cashierId,
                    CustomerName = dto.CustomerName,
                    Phone = dto.Phone,
                    Address = dto.Address,
                    PaymentMethod = dto.PaymentMethod,
                    Status = "pending",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    OrderItems = new List<OrderItem>()
                };

                decimal total = 0;
                var productIds = new HashSet<int>();

                foreach (var item in dto.Items)
                {
                    // Find the variant
                    var variant = await _context.ProductVariants
                        .FirstOrDefaultAsync(v => v.ProductId == item.ProductId
                                               && v.Size == item.SelectedSize
                                               && v.ColorName == item.SelectedColorName);
                    if (variant == null)
                        throw new InvalidOperationException($"Variant not found: Product {item.ProductId}, Size {item.SelectedSize}, Color {item.SelectedColorName}");
                    if (variant.Stock < item.Quantity)
                        throw new InvalidOperationException($"Insufficient stock for variant: {variant.Size} {variant.ColorName}. Available: {variant.Stock}");

                    // Deduct stock
                    variant.Stock -= item.Quantity;
                    variant.UpdatedAt = DateTime.UtcNow;
                    productIds.Add(item.ProductId);

                    // Add order item
                    order.OrderItems.Add(new OrderItem
                    {
                        ProductId = item.ProductId,
                        Quantity = item.Quantity,
                        Price = item.Price,
                        SelectedSize = item.SelectedSize,
                        SelectedColorName = item.SelectedColorName
                    });

                    total += item.Price * item.Quantity;
                }

                order.TotalAmount = total;
                _context.Orders.Add(order);
                await _context.SaveChangesAsync();

                // Update product total stock for each affected product
                foreach (var pid in productIds)
                {
                    await UpdateProductTotalStockAsync(pid);
                }

                await transaction.CommitAsync();
                return order;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
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
                    ProductName = oi.Product?.NameEnglish ?? oi.Product?.NameKhmer,
                    Quantity = oi.Quantity,
                    Price = oi.Price,
                    SelectedSize = oi.SelectedSize,
                    SelectedColorName = oi.SelectedColorName
                }).ToList()
            };
        }
        public async Task<User?> GetUserByIdAsync(int id)
        {
            return await _context.Users.FindAsync(id);
        }
    }
}