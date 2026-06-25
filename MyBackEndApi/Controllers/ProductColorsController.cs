using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyBackEndApi.Data;
using MyBackEndApi.DTOs.Color;
using MyBackEndApi.Models;
using Microsoft.EntityFrameworkCore;
namespace MyBackEndApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "admin")] // Only admins can manage colors
    public class ProductColorsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ProductColorsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/productcolors
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductColorDto>>> GetAllColors()
        {
            var colors = await _context.ProductColors
                .Select(pc => new ProductColorDto
                {
                    Id = pc.Id,
                    ProductId = pc.product_id,
                    ColorName = pc.ColorName,
                    ColorHex = pc.ColorHex
                })
                .ToListAsync();
            return Ok(colors);
        }

        // GET: api/productcolors/product/{productId}
        [HttpGet("product/{productId}")]
        public async Task<ActionResult<IEnumerable<ProductColorDto>>> GetColorsByProduct(int productId)
        {
            var colors = await _context.ProductColors
                .Where(pc => pc.product_id == productId)
                .Select(pc => new ProductColorDto
                {
                    Id = pc.Id,
                    ProductId = pc.product_id,
                    ColorName = pc.ColorName,
                    ColorHex = pc.ColorHex
                })
                .ToListAsync();
            return Ok(colors);
        }

        // POST: api/productcolors
        [HttpPost]
        public async Task<IActionResult> CreateColor([FromBody] CreateProductColorDto dto)
        {
            // Check if product exists
            var product = await _context.Products.FindAsync(dto.ProductId);
            if (product == null)
                return NotFound(new { message = "Product not found" });

            // Check for duplicate (same product, same color name)
            var exists = await _context.ProductColors.AnyAsync(pc => pc.product_id == dto.ProductId && pc.ColorName == dto.ColorName);
            if (exists)
                return Conflict(new { message = "Color already exists for this product" });

            var color = new ProductColor
            {
                product_id = dto.ProductId,
                ColorName = dto.ColorName,
                ColorHex = dto.ColorHex
            };
            _context.ProductColors.Add(color);
            await _context.SaveChangesAsync();

            return Ok(new ProductColorDto
            {
                Id = color.Id,
                ProductId = color.product_id,
                ColorName = color.ColorName,
                ColorHex = color.ColorHex
            });
        }

        // PUT: api/productcolors/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateColor(int id, [FromBody] UpdateProductColorDto dto)
        {
            var color = await _context.ProductColors.FindAsync(id);
            if (color == null)
                return NotFound(new { message = "Color not found" });

            // Check for duplicate (different id, same product and color name)
            var duplicate = await _context.ProductColors.AnyAsync(pc => pc.Id != id && pc.product_id == color.product_id && pc.ColorName == dto.ColorName);
            if (duplicate)
                return Conflict(new { message = "Another color with same name already exists for this product" });

            color.ColorName = dto.ColorName;
            color.ColorHex = dto.ColorHex;
            await _context.SaveChangesAsync();

            return Ok(new ProductColorDto
            {
                Id = color.Id,
                ProductId = color.product_id,
                ColorName = color.ColorName,
                ColorHex = color.ColorHex
            });
        }

        // DELETE: api/productcolors/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteColor(int id)
        {
            var color = await _context.ProductColors.FindAsync(id);
            if (color == null)
                return NotFound(new { message = "Color not found" });

            _context.ProductColors.Remove(color);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Color deleted successfully" });
        }
    }
}
