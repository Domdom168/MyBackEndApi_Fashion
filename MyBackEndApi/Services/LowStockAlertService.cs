using MyBackEndApi.Data;
using MyBackEndApi.DTOs.LowStockAlert;
using MyBackEndApi.Models;
using Microsoft.EntityFrameworkCore;
namespace MyBackEndApi.Services
{
    public class LowStockAlertService : ILowStockAlertService
    {
        private readonly AppDbContext _context;

        public LowStockAlertService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<LowStockAlertDto>> GetAllAlertsAsync()
        {
            var alerts = await _context.LowStockAlerts
                .Include(a => a.Product)
                .Select(a => new LowStockAlertDto
                {
                    Id = a.Id,
                    ProductId = a.ProductId,
                    ProductName = a.Product.NameEnglish ?? a.Product.NameKhmer,
                    CurrentStock = a.Product.Stock,
                    Threshold = a.Threshold,
                    IsActive = a.IsActive,
                    CreatedAt = a.CreatedAt
                })
                .ToListAsync();
            return alerts;
        }

        public async Task<LowStockAlertDto?> GetAlertByProductIdAsync(int productId)
        {
            var alert = await _context.LowStockAlerts
                .Include(a => a.Product)
                .FirstOrDefaultAsync(a => a.ProductId == productId);
            if (alert == null) return null;

            return new LowStockAlertDto
            {
                Id = alert.Id,
                ProductId = alert.ProductId,
                ProductName = alert.Product.NameEnglish ?? alert.Product.NameKhmer,
                CurrentStock = alert.Product.Stock,
                Threshold = alert.Threshold,
                IsActive = alert.IsActive,
                CreatedAt = alert.CreatedAt
            };
        }

        public async Task<LowStockAlertDto> CreateOrUpdateAlertAsync(int productId, int threshold)
        {
            var existing = await _context.LowStockAlerts
                .FirstOrDefaultAsync(a => a.ProductId == productId);
            if (existing != null)
            {
                existing.Threshold = threshold;
                existing.IsActive = true;
                await _context.SaveChangesAsync();
                return await GetAlertByProductIdAsync(productId);
            }

            var product = await _context.Products.FindAsync(productId);
            if (product == null) throw new ArgumentException("Product not found");

            var newAlert = new LowStockAlert
            {
                ProductId = productId,
                Threshold = threshold,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            _context.LowStockAlerts.Add(newAlert);
            await _context.SaveChangesAsync();
            return await GetAlertByProductIdAsync(productId);
        }

        public async Task<bool> DeleteAlertAsync(int id)
        {
            var alert = await _context.LowStockAlerts.FindAsync(id);
            if (alert == null) return false;
            _context.LowStockAlerts.Remove(alert);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
