using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyBackEndApi.Data;
using MyBackEndApi.DTOs.Size;
using MyBackEndApi.Models;
using Microsoft.EntityFrameworkCore;
namespace MyBackEndApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "admin")] // Only admins can manage sizes
    public class ProductSizesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ProductSizesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/productsizes
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductSizeDto>>> GetAllSizes()
        {
            var sizes = await _context.ProductSizes
                .Select(ps => new ProductSizeDto
                {
                    Id = ps.Id,
                    ProductId = ps.ProductId,
                    Size = ps.Size
                })
                .ToListAsync();
            return Ok(sizes);
        }

        // GET: api/productsizes/product/{productId}
        [HttpGet("product/{productId}")]
        public async Task<ActionResult<IEnumerable<ProductSizeDto>>> GetSizesByProduct(int productId)
        {
            var sizes = await _context.ProductSizes
                .Where(ps => ps.ProductId == productId)
                .Select(ps => new ProductSizeDto
                {
                    Id = ps.Id,
                    ProductId = ps.ProductId,
                    Size = ps.Size
                })
                .ToListAsync();
            return Ok(sizes);
        }

        // POST: api/productsizes
        [HttpPost]
        public async Task<IActionResult> CreateSize([FromBody] CreateProductSizeDto dto)
        {
            // Check if product exists
            var product = await _context.Products.FindAsync(dto.ProductId);
            if (product == null)
                return NotFound(new { message = "Product not found" });

            // Check for duplicate size for the same product
            var exists = await _context.ProductSizes.AnyAsync(ps => ps.ProductId == dto.ProductId && ps.Size == dto.Size);
            if (exists)
                return Conflict(new { message = "Size already exists for this product" });

            var size = new ProductSize
            {
                ProductId = dto.ProductId,
                Size = dto.Size
            };
            _context.ProductSizes.Add(size);
            await _context.SaveChangesAsync();

            return Ok(new ProductSizeDto
            {
                Id = size.Id,
                ProductId = size.ProductId,
                Size = size.Size
            });
        }

        // PUT: api/productsizes/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateSize(int id, [FromBody] UpdateProductSizeDto dto)
        {
            var size = await _context.ProductSizes.FindAsync(id);
            if (size == null)
                return NotFound(new { message = "Size not found" });

            // Check for duplicate (different id, same product and size)
            var duplicate = await _context.ProductSizes.AnyAsync(ps => ps.Id != id && ps.ProductId == size.ProductId && ps.Size == dto.Size);
            if (duplicate)
                return Conflict(new { message = "Another size with same value already exists for this product" });

            size.Size = dto.Size;
            await _context.SaveChangesAsync();

            return Ok(new ProductSizeDto
            {
                Id = size.Id,
                ProductId = size.ProductId,
                Size = size.Size
            });
        }

        // DELETE: api/productsizes/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSize(int id)
        {
            var size = await _context.ProductSizes.FindAsync(id);
            if (size == null)
                return NotFound(new { message = "Size not found" });

            _context.ProductSizes.Remove(size);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Size deleted successfully" });
        }
    }
}
