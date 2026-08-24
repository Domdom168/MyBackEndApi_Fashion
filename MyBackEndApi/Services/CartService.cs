using Microsoft.EntityFrameworkCore;
using MyBackEndApi.Data;
using MyBackEndApi.DTOs.Cart;
using MyBackEndApi.Models;

namespace MyBackEndApi.Services
{
    public class CartService : ICartService
    {
        private readonly AppDbContext _context;

        public CartService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<CartItemDto>> GetCartAsync(int userId)
        {
            return await _context.Carts
                .Where(c => c.UserId == userId)
                .Include(c => c.Product)
                .ThenInclude(p => p.ProductImages)
                .Select(c => new CartItemDto
                {
                    Id = c.Id,
                    ProductId = c.ProductId,
                    ProductName = c.Product.NameEnglish ?? c.Product.NameKhmer,
                    ProductImage = c.Product.ProductImages.OrderBy(pi => pi.DisplayOrder).Select(pi => pi.ImageUrl).FirstOrDefault() ?? c.Product.ImageUrl,
                    Price = c.Product.Price,
                    Quantity = c.Quantity,
                    SelectedSize = c.SelectedSize,
                    SelectedColorName = c.SelectedColorName
                })
                .ToListAsync();
        }

        public async Task AddToCartAsync(int userId, AddToCartDto dto)
        {
            // 1. Validate product exists
            var product = await _context.Products.FindAsync(dto.ProductId);
            if (product == null)
                throw new ArgumentException("Product not found.");

            // 2. Validate variant (if size/color provided)
            if (!string.IsNullOrEmpty(dto.SelectedSize) && !string.IsNullOrEmpty(dto.SelectedColorName))
            {
                var variant = await _context.ProductVariants
                    .FirstOrDefaultAsync(v => v.ProductId == dto.ProductId
                                           && v.Size == dto.SelectedSize
                                           && v.ColorName == dto.SelectedColorName);
                if (variant == null)
                    throw new ArgumentException("Selected size/color combination not available.");
                if (variant.Stock < dto.Quantity)
                    throw new InvalidOperationException($"Insufficient stock for variant: {variant.Size} {variant.ColorName}. Available: {variant.Stock}");
            }
            else
            {
                // If no variant selected, check product stock (fallback)
                if (product.Stock < dto.Quantity)
                    throw new InvalidOperationException($"Insufficient stock for product: {product.NameEnglish}. Available: {product.Stock}");
            }

            // 3. Check if the same item already exists in cart (same product, size, color)
            var existing = await _context.Carts
                .FirstOrDefaultAsync(c => c.UserId == userId
                                       && c.ProductId == dto.ProductId
                                       && c.SelectedSize == dto.SelectedSize
                                       && c.SelectedColorName == dto.SelectedColorName);

            if (existing != null)
            {
                // Update quantity
                existing.Quantity += dto.Quantity;
                await _context.SaveChangesAsync();
                return;
            }

            // 4. Add new cart item
            var cartItem = new Cart
            {
                UserId = userId,
                ProductId = dto.ProductId,
                Quantity = dto.Quantity,
                SelectedSize = dto.SelectedSize,
                SelectedColorName = dto.SelectedColorName,
                SelectedColorHex = dto.SelectedColorHex,
                CreatedAt = DateTime.UtcNow
            };

            _context.Carts.Add(cartItem);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateCartItemAsync(int userId, int cartItemId, int quantity)
        {
            var cartItem = await _context.Carts
                .Include(c => c.Product)
                .FirstOrDefaultAsync(c => c.Id == cartItemId && c.UserId == userId);
            if (cartItem == null)
                throw new ArgumentException("Cart item not found.");

            // Validate stock (based on variant or product)
            if (!string.IsNullOrEmpty(cartItem.SelectedSize) && !string.IsNullOrEmpty(cartItem.SelectedColorName))
            {
                var variant = await _context.ProductVariants
                    .FirstOrDefaultAsync(v => v.ProductId == cartItem.ProductId
                                           && v.Size == cartItem.SelectedSize
                                           && v.ColorName == cartItem.SelectedColorName);
                if (variant == null)
                    throw new ArgumentException("Variant no longer available.");
                if (variant.Stock < quantity)
                    throw new InvalidOperationException($"Insufficient stock. Available: {variant.Stock}");
            }
            else
            {
                if (cartItem.Product.Stock < quantity)
                    throw new InvalidOperationException($"Insufficient stock. Available: {cartItem.Product.Stock}");
            }

            cartItem.Quantity = quantity;
            await _context.SaveChangesAsync();
        }

        public async Task RemoveCartItemAsync(int userId, int cartItemId)
        {
            var cartItem = await _context.Carts
                .FirstOrDefaultAsync(c => c.Id == cartItemId && c.UserId == userId);
            if (cartItem == null)
                throw new ArgumentException("Cart item not found.");

            _context.Carts.Remove(cartItem);
            await _context.SaveChangesAsync();
        }

        public async Task ClearCartAsync(int userId)
        {
            var cartItems = await _context.Carts
                .Where(c => c.UserId == userId)
                .ToListAsync();

            _context.Carts.RemoveRange(cartItems);
            await _context.SaveChangesAsync();
        }
    }
}