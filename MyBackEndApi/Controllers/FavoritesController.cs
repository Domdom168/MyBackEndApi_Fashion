using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyBackEndApi.Data;
using MyBackEndApi.DTOs.Favorite;
using MyBackEndApi.Models;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
namespace MyBackEndApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FavoritesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public FavoritesController(AppDbContext context)
        {
            _context = context;
        }

        // Helper to get current user ID from JWT claims
        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)
                              ?? User.FindFirst(JwtRegisteredClaimNames.Sub);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                throw new UnauthorizedAccessException("Invalid user token");
            return userId;
        }

        // GET: api/favorites
        [HttpGet]
        public async Task<ActionResult<IEnumerable<FavoriteResponseDto>>> GetFavorites()
        {
            var userId = GetCurrentUserId();

            var favorites = await _context.Favorites
                .Where(f => f.UserId == userId)
                .Include(f => f.Product)
                .ThenInclude(p => p.ProductImages)
                .Select(f => new FavoriteResponseDto
                {
                    Id = f.Id,
                    ProductId = f.ProductId,
                    ProductName = f.Product.NameEnglish ?? f.Product.NameKhmer,
                    ProductImage = f.Product.ProductImages.OrderBy(pi => pi.DisplayOrder).Select(pi => pi.ImageUrl).FirstOrDefault() ?? f.Product.ImageUrl,
                    Price = f.Product.Price,
                    CreatedAt = f.CreatedAt
                })
                .ToListAsync();

            return Ok(favorites);
        }

        // POST: api/favorites/{productId}
        [HttpPost("{productId}")]
        public async Task<IActionResult> AddFavorite(int productId)
        {
            var userId = GetCurrentUserId();
            // Verify customer exists
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return Unauthorized(new { message = "Invalid customer account. Please login as a customer." });
            // Check if product exists
            var product = await _context.Products.FindAsync(productId);
            if (product == null)
                return NotFound(new { message = "Product not found" });

            // Check if already in favorites
            var exists = await _context.Favorites.AnyAsync(f => f.UserId == userId && f.ProductId == productId);
            if (exists)
                return Conflict(new { message = "Product already in favorites" });

            var favorite = new Favorite
            {
                UserId = userId,
                ProductId = productId,
                CreatedAt = DateTime.UtcNow
            };
            _context.Favorites.Add(favorite);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Added to favorites" });
        }

        // DELETE: api/favorites/{productId}
        [HttpDelete("{productId}")]
        public async Task<IActionResult> RemoveFavorite(int productId)
        {
            var userId = GetCurrentUserId();

            var favorite = await _context.Favorites
                .FirstOrDefaultAsync(f => f.UserId == userId && f.ProductId == productId);
            if (favorite == null)
                return NotFound(new { message = "Favorite not found" });

            _context.Favorites.Remove(favorite);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Removed from favorites" });
        }
    }
}
