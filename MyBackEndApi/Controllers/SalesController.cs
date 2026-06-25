using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyBackEndApi.Data;
using MyBackEndApi.DTOs.Order;

using Microsoft.EntityFrameworkCore;
namespace MyBackEndApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "admin,cashier")]
    public class SalesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public SalesController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<PagedResultDto<SaleResponseDto>>> GetSales([FromQuery] SaleFilterDto filter)
        {
            var query = _context.Sales
                .Include(s => s.Cashier)
                .Include(s => s.Order)
                .AsQueryable();

            if (filter.FromDate.HasValue)
                query = query.Where(s => s.SaleDate >= filter.FromDate.Value);
            if (filter.ToDate.HasValue)
                query = query.Where(s => s.SaleDate <= filter.ToDate.Value);
            if (!string.IsNullOrEmpty(filter.PaymentMethod))
                query = query.Where(s => s.PaymentMethod == filter.PaymentMethod);
            if (filter.CashierId.HasValue)
                query = query.Where(s => s.CashierId == filter.CashierId);

            var total = await query.CountAsync();
            var items = await query
                .OrderByDescending(s => s.SaleDate)
                .ThenByDescending(s => s.SaleTime)
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .Select(s => new SaleResponseDto
                {
                    Id = s.Id,
                    OrderId = s.OrderId,
                    CashierId = s.CashierId,
                    CashierName = s.Cashier != null ? s.Cashier.Name : null,
                    TotalAmount = s.TotalAmount,
                    PaymentMethod = s.PaymentMethod,
                    Status = s.Order != null ? s.Order.Status : null,
                    Phone = s.Order != null ? s.Order.Phone : null,
                    CustomerName = s.Order != null ? s.Order.CustomerName : null,
                    SaleDate = s.SaleDate,
                    SaleTime = s.SaleTime, 
                    CreatedAt = s.CreatedAt
                })
                .ToListAsync();

            return Ok(new PagedResultDto<SaleResponseDto>
            {
                Page = filter.Page,
                PageSize = filter.PageSize,
                TotalCount = total,
                Items = items
            });
        }

        [HttpGet("summary")]
        public async Task<ActionResult<SalesSummaryDto>> GetSalesSummary([FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate)
        {
            var query = _context.Sales.AsQueryable();
            if (fromDate.HasValue) query = query.Where(s => s.SaleDate >= fromDate.Value);
            if (toDate.HasValue) query = query.Where(s => s.SaleDate <= toDate.Value);

            var sales = await query.ToListAsync();
            var totalRevenue = sales.Sum(s => s.TotalAmount);
            var totalOrders = sales.Count;
            var avgOrderValue = totalOrders > 0 ? totalRevenue / totalOrders : 0;

            var paymentBreakdown = sales
                .GroupBy(s => s.PaymentMethod ?? "Unknown")
                .ToDictionary(g => g.Key, g => g.Sum(s => s.TotalAmount));

            var dailySales = sales
                .GroupBy(s => s.SaleDate.Date)
                .Select(g => new DailySalesDto
                {
                    Date = g.Key,
                    Revenue = g.Sum(s => s.TotalAmount),
                    Orders = g.Count()
                })
                .OrderBy(d => d.Date)
                .ToList();

            return Ok(new SalesSummaryDto
            {
                TotalRevenue = totalRevenue,
                TotalOrders = totalOrders,
                AverageOrderValue = avgOrderValue,
                PaymentMethodBreakdown = paymentBreakdown,
                DailySales = dailySales
            });
        }
    }
}
