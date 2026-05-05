using MyBackEndApi.Data;
using MyBackEndApi.DTOs.Order;
using Microsoft.EntityFrameworkCore;

namespace MyBackEndApi.Services
{
    public class SalesService: ISalesService
    {
        private readonly AppDbContext _context;

        public SalesService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<PagedResultDto<SaleResponseDto>> GetSalesAsync(SaleFilterDto filter)
        {
            var query = _context.Sales
                .Include(s => s.Cashier)
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
                    SaleDate = s.SaleDate,
                    SaleTime = s.SaleTime,
                    CreatedAt = s.CreatedAt
                })
                .ToListAsync();

            return new PagedResultDto<SaleResponseDto>
            {
                Page = filter.Page,
                PageSize = filter.PageSize,
                TotalCount = total,
                Items = items
            };
        }

        public async Task<SalesSummaryDto> GetSalesSummaryAsync(DateTime? fromDate, DateTime? toDate)
        {
            var query = _context.Sales.AsQueryable();
            if (fromDate.HasValue) query = query.Where(s => s.SaleDate >= fromDate.Value);
            if (toDate.HasValue) query = query.Where(s => s.SaleDate <= toDate.Value);

            var sales = await query.ToListAsync();
            var totalRevenue = sales.Sum(s => s.TotalAmount);
            var totalOrders = sales.Count;
            var avgOrderValue = totalOrders > 0 ? totalRevenue / totalOrders : 0;

            var paymentMethodBreakdown = sales
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

            return new SalesSummaryDto
            {
                TotalRevenue = totalRevenue,
                TotalOrders = totalOrders,
                AverageOrderValue = avgOrderValue,
                PaymentMethodBreakdown = paymentMethodBreakdown,
                DailySales = dailySales
            };
        }
    }
}
