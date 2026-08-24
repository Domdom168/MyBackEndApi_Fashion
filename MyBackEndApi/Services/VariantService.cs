using Microsoft.EntityFrameworkCore;
using MyBackEndApi.Data;
using MyBackEndApi.DTOs;
using MyBackEndApi.DTOs.ProductVariant;
using MyBackEndApi.Models;

namespace MyBackEndApi.Services
{
    public class VariantService : IVariantService
    {
        private readonly AppDbContext _context;

        public VariantService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<VariantResponseDto>> GetAllVariantsAsync()
        {
            return await _context.ProductVariants
                .Select(v => MapToDto(v))
                .ToListAsync();
        }

        public async Task<VariantResponseDto?> GetVariantByIdAsync(int id)
        {
            var variant = await _context.ProductVariants.FindAsync(id);
            return variant == null ? null : MapToDto(variant);
        }

        public async Task<IEnumerable<VariantResponseDto>> GetVariantsByProductAsync(int productId)
        {
            return await _context.ProductVariants
                .Where(v => v.ProductId == productId)
                .Select(v => MapToDto(v))
                .ToListAsync();
        }

        public async Task<VariantResponseDto> CreateVariantAsync(VariantCreateDto dto)
        {
            var product = await _context.Products.FindAsync(dto.ProductId);
            if (product == null)
                throw new ArgumentException("Product not found");

            var exists = await _context.ProductVariants.AnyAsync(v =>
                v.ProductId == dto.ProductId && v.Size == dto.Size && v.ColorName == dto.ColorName);
            if (exists)
                throw new InvalidOperationException("Variant already exists for this product, size and color.");

            var variant = new ProductVariant
            {
                ProductId = dto.ProductId,
                Size = dto.Size,
                ColorName = dto.ColorName,
                ColorHex = dto.ColorHex,
                Stock = dto.Stock,
                Price = dto.Price,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.ProductVariants.Add(variant);
            await _context.SaveChangesAsync();

            // Update product total stock
            await UpdateProductTotalStockAsync(dto.ProductId);

            return MapToDto(variant);
        }

        public async Task<VariantResponseDto> UpdateVariantAsync(int id, VariantUpdateDto dto)
        {
            var variant = await _context.ProductVariants.FindAsync(id);
            if (variant == null)
                throw new ArgumentException("Variant not found");

            var duplicate = await _context.ProductVariants.AnyAsync(v =>
                v.Id != id && v.ProductId == variant.ProductId && v.Size == dto.Size && v.ColorName == dto.ColorName);
            if (duplicate)
                throw new InvalidOperationException("Another variant with same size and color exists.");

            variant.Size = dto.Size;
            variant.ColorName = dto.ColorName;
            variant.ColorHex = dto.ColorHex;
            variant.Stock = dto.Stock;
            variant.Price = dto.Price;
            variant.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // Update product total stock
            await UpdateProductTotalStockAsync(variant.ProductId);

            return MapToDto(variant);
        }

        public async Task<bool> DeleteVariantAsync(int id)
        {
            var variant = await _context.ProductVariants.FindAsync(id);
            if (variant == null) return false;

            var productId = variant.ProductId;
            _context.ProductVariants.Remove(variant);
            await _context.SaveChangesAsync();

            // Update product total stock
            await UpdateProductTotalStockAsync(productId);

            return true;
        }

        // Helper to update product total stock
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

        private static VariantResponseDto MapToDto(ProductVariant v) => new()
        {
            Id = v.Id,
            ProductId = v.ProductId,
            Size = v.Size,
            ColorName = v.ColorName,
            ColorHex = v.ColorHex,
            Stock = v.Stock,
            Price = v.Price,
            CreatedAt = v.CreatedAt,
            UpdatedAt = v.UpdatedAt
        };
    }
}