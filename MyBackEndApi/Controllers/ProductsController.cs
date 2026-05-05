using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyBackEndApi.Data;
using MyBackEndApi.DTOs.Product;
using MyBackEndApi.Models;
using MyBackEndApi.Services;
namespace MyBackEndApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env; // New field private readonly IWebHostEnvironment _env; // New field
        private readonly IProductService _productService;
        public ProductsController(AppDbContext context, IWebHostEnvironment env, IProductService productService)
        {
            _context = context;
            _env = env;
            _productService = productService;
        }


        // GET: api/products
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductListDto>>> GetProducts()
        {
            var products = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.ProductImages)
                .Select(p => new ProductListDto
                {
                    Id = p.Id,
                    // Get the first image URL (lowest display_order) or fallback to p.ImageUrl
                    ImageUrl = p.ProductImages
                                   .OrderBy(pi => pi.DisplayOrder)
                                   .Select(pi => pi.ImageUrl)
                                   .FirstOrDefault() ?? p.ImageUrl,

                    // Choose which name to display – here we use English, you can change to Khmer or combine
                    NameKhmer = p.NameKhmer,
                    NameEnglish= p.NameEnglish,
                    CategoryId=p.category_id,
                 
                    CategoryEnglish =  p.Category.NameEnglish,
                    CategoryKhmer = p.Category.NameKhmer,
                    DescriptionEnglish = p.DescriptionEnglish,
                    DescriptionKhmer = p.DescriptionKhmer,
                    Price = p.Price,
                    Stock = p.Stock,
                    Rating = p.Rating,
                    // Service placeholder – you can change this logic if "service" comes from another table
                    Service = p.Stock > 0 ? "Available" : "Out of Stock"
                })
                .ToListAsync();

            return Ok(products);
        }


        [HttpPost]
        public async Task<ActionResult<ProductListDto>> CreateProduct([FromForm] CreateProductDto productDto)
        {
            // 1. Validate category
            var category = await _context.Categories.FindAsync(productDto.CategoryId);
            if (category == null)
                return BadRequest("Invalid category ID.");

            // 2. Create product entity
            var product = new Product
            {
                // Store the provided name in English (you can also map to Khmer if you have a separate field)
                NameEnglish = productDto.NameEnglish,
                DescriptionKhmer = productDto.DescriptionKhmer,
                DescriptionEnglish = productDto.DescriptionEnglish,
                // If you want to also store a Khmer name, add a separate field in the DTO
                NameKhmer = productDto.NameKhmer, // optional: store same in Khmer if needed
                category_id = productDto.CategoryId,
                Price = productDto.Price,
                Stock = productDto.Stock,
                Rating = productDto.Rating ?? 0,
                created_at = DateTime.Now,
                updated_at = DateTime.Now
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync(); // get product ID

            // Prepare upload folder: wwwroot/images/products
            var uploadsFolder = Path.Combine(_env.WebRootPath, "images", "products");
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            // Save main image
            if (productDto.ImageFile != null && productDto.ImageFile.Length > 0)
            {
                var fileName = $"{product.Id}_{Guid.NewGuid()}{Path.GetExtension(productDto.ImageFile.FileName)}";
                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await productDto.ImageFile.CopyToAsync(stream);
                }

                _context.ProductImages.Add(new ProductImage
                {
                    product_id = product.Id,
                    ImageUrl = $"/images/products/{fileName}",
                    DisplayOrder = 0
                });
            }

            // Save additional images
            if (productDto.AdditionalImages != null)
            {
                int order = 1;
                foreach (var img in productDto.AdditionalImages)
                {
                    if (img.Length > 0)
                    {
                        var fileName = $"{product.Id}_{Guid.NewGuid()}{Path.GetExtension(img.FileName)}";
                        var filePath = Path.Combine(uploadsFolder, fileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await img.CopyToAsync(stream);
                        }

                        _context.ProductImages.Add(new ProductImage
                        {
                            product_id = product.Id,
                            ImageUrl = $"/images/products/{fileName}",
                            DisplayOrder = order++
                        });
                    }
                }
            }

            // 5. Handle colors (optional)
            if (productDto.Colors != null)
            {
                foreach (var colorName in productDto.Colors)
                {
                    _context.ProductColors.Add(new ProductColor
                    {
                        product_id = product.Id,
                        ColorName = colorName,
                        ColorHex = null // can be set later
                    });
                }
            }

            // 6. Handle sizes (optional)
            if (productDto.Sizes != null)
            {
                foreach (var size in productDto.Sizes)
                {
                    _context.ProductSizes.Add(new ProductSize
                    {
                        ProductId = product.Id,
                        Size = size
                    });
                }
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                // Get the detailed inner exception
                var inner = ex.InnerException?.Message;
                // Log or return it to the client (in development)
                return BadRequest($"Database error: {inner}");
            }

            // 7. Return the created product in the same format as GET
            var createdProductDto = new ProductListDto
            {
                Id = product.Id,
                ImageUrl = _context.ProductImages
                               .Where(pi => pi.product_id == product.Id)
                               .OrderBy(pi => pi.DisplayOrder)
                               .Select(pi => pi.ImageUrl)
                               .FirstOrDefault(),
                NameEnglish = product.NameEnglish,
                NameKhmer= product.NameKhmer,
                CategoryEnglish = category.NameEnglish,
                CategoryKhmer= category.NameKhmer,
                Price = product.Price,
                Stock = product.Stock,
                Rating = product.Rating,
                Service = product.Stock > 0 ? "Available" : "Out of Stock"
            };

            return CreatedAtAction(nameof(GetProductById), new { id = product.Id }, createdProductDto);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ProductListDto>> GetProductById(int id)
        {
            var product = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.ProductImages)
                .Where(p => p.Id == id)
                .Select(p => new ProductListDto
                {
                    Id = p.Id,
                    ImageUrl = p.ProductImages.OrderBy(pi => pi.DisplayOrder)
                                              .Select(pi => pi.ImageUrl)
                                              .FirstOrDefault() ?? p.ImageUrl,
                    NameKhmer = p.NameKhmer,
                    NameEnglish= p.NameEnglish,
                    CategoryEnglish =  p.Category.NameEnglish,
                    CategoryKhmer= p.Category.NameKhmer,
                    Price = p.Price,
                    Stock = p.Stock,
                    Rating = p.Rating,
                    Service = p.Stock > 0 ? "Available" : "Out of Stock"
                })
                .FirstOrDefaultAsync();

            if (product == null)
                return NotFound();

            return Ok(product);
        }


        [HttpPut("{id}")]
        public async Task<ActionResult<ProductListDto>> UpdateProduct(int id, [FromForm] CreateProductDto productDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var updated = await _productService.UpdateProductAsync(id, productDto);
                if (updated == null)
                    return NotFound();

                return Ok(updated);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while updating the product.");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var deleted = await _productService.DeleteProductAsync(id);
            if (!deleted)
                return NotFound();

            return NoContent();
        }

    }
}
