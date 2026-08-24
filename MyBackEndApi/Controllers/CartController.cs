using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyBackEndApi.Data;
using MyBackEndApi.DTOs.Cart;
using MyBackEndApi.Models;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
namespace MyBackEndApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "user")]
    public class CartController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CartController(AppDbContext context)
        {
            _context = context;
        }

        private int GetCurrentUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst(JwtRegisteredClaimNames.Sub);
            if (claim == null || !int.TryParse(claim.Value, out int userId))
                throw new UnauthorizedAccessException();
            return userId;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<CartItemDto>>> GetCart()
        {
            var userId = GetCurrentUserId();
            var cart = await _context.Carts
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
            return Ok(cart);
        }

        [HttpPost]
        public async Task<IActionResult> AddToCart([FromBody] AddToCartDto dto)
        {
            var userId = GetCurrentUserId();

            var product = await _context.Products.FindAsync(dto.ProductId);
            if (product == null)
                return NotFound(new { message = "Product not found" });
            if (product.Stock < dto.Quantity)
                return BadRequest(new { message = "Insufficient stock" });

            var existing = await _context.Carts
                .FirstOrDefaultAsync(c => c.UserId == userId && c.ProductId == dto.ProductId
                    && c.SelectedSize == dto.SelectedSize && c.SelectedColorName == dto.SelectedColorName);
            if (existing != null)
            {
                existing.Quantity += dto.Quantity;
                if (product.Stock < existing.Quantity)
                    return BadRequest(new { message = "Insufficient stock for total quantity" });
            }
            else
            {
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
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = "Item added to cart" });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCartItem(int id, [FromBody] UpdateCartItemDto dto)
        {
            var userId = GetCurrentUserId();
            var cartItem = await _context.Carts
                .Include(c => c.Product)
                .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);
            if (cartItem == null)
                return NotFound(new { message = "Cart item not found" });

            if (cartItem.Product.Stock < dto.Quantity)
                return BadRequest(new { message = "Insufficient stock" });

            cartItem.Quantity = dto.Quantity;
            await _context.SaveChangesAsync();
            return Ok(new { message = "Cart updated" });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> RemoveCartItem(int id)
        {
            var userId = GetCurrentUserId();
            var cartItem = await _context.Carts.FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);
            if (cartItem == null)
                return NotFound(new { message = "Cart item not found" });
            _context.Carts.Remove(cartItem);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Item removed from cart" });
        }

        [HttpDelete]
        public async Task<IActionResult> ClearCart()
        {
            var userId = GetCurrentUserId();
            var cartItems = await _context.Carts.Where(c => c.UserId == userId).ToListAsync();
            _context.Carts.RemoveRange(cartItems);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Cart cleared" });
        }
    }
}
  