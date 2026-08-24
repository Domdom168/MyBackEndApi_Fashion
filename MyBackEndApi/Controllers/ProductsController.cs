using Microsoft.AspNetCore.Mvc;
using MyBackEndApi.DTOs.Product;
using MyBackEndApi.Services;
using System;

namespace MyBackEndApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductsController(IProductService productService)
        {
            _productService = productService;
        }

        // GET: api/products
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductListDto>>> GetProducts()
        {
            var products = await _productService.GetAllProductsAsync();
            return Ok(products);
        }

        // GET: api/products/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<ProductListDto>> GetProductById(int id)
        {
            var product = await _productService.GetProductByIdAsync(id);
            if (product == null)
                return NotFound();
            return Ok(product);
        }

        // POST: api/products
        [HttpPost]
        public async Task<ActionResult<ProductListDto>> CreateProduct([FromForm] ProductCreateDto productDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var created = await _productService.CreateProductAsync(productDto);
                return CreatedAtAction(nameof(GetProductById), new { id = created.Id }, created);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while creating the product.", detail = ex.Message, inner = ex.InnerException?.Message });
            }
        }

        // PUT: api/products/{id}
        [HttpPut("{id}")]
        public async Task<ActionResult<ProductListDto>> UpdateProduct(int id, [FromForm] ProductCreateDto productDto)
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
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while updating the product.", detail = ex.Message });
            }
        }

        // DELETE: api/products/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            try
            {
                var deleted = await _productService.DeleteProductAsync(id);
                if (!deleted)
                    return NotFound();
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        [HttpDelete("image/{id}")]
        public async Task<IActionResult> DeleteImage(int id)
        {
            var deleted = await _productService.DeleteProductImageAsync(id);
            if (!deleted) return NotFound();
            return NoContent();
        }
        [HttpPatch("{id}/restore")]
        public async Task<IActionResult> Restore(int id)
        {
            var restored = await _productService.RestoreProductAsync(id);
            if (!restored) return NotFound();
            return Ok(new { message = "Product restored" });
        }
    }
}