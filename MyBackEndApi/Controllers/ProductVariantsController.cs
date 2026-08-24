using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyBackEndApi.DTOs;
using MyBackEndApi.DTOs.ProductVariant;
using MyBackEndApi.Services;

namespace MyBackEndApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "admin,cashier")] // Only admins can manage variants
    public class ProductVariantsController : ControllerBase
    {
        private readonly IVariantService _variantService;

        public ProductVariantsController(IVariantService variantService)
        {
            _variantService = variantService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<VariantResponseDto>>> GetAll()
        {
            return Ok(await _variantService.GetAllVariantsAsync());
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<VariantResponseDto>> GetById(int id)
        {
            var variant = await _variantService.GetVariantByIdAsync(id);
            if (variant == null) return NotFound();
            return Ok(variant);
        }

        [HttpGet("product/{productId}")]
        public async Task<ActionResult<IEnumerable<VariantResponseDto>>> GetByProduct(int productId)
        {
            return Ok(await _variantService.GetVariantsByProductAsync(productId));
        }

        [HttpPost]
        public async Task<ActionResult<VariantResponseDto>> Create([FromBody] VariantCreateDto dto)
        {
            try
            {
                var result = await _variantService.CreateVariantAsync(dto);
                return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<VariantResponseDto>> Update(int id, [FromBody] VariantUpdateDto dto)
        {
            try
            {
                var result = await _variantService.UpdateVariantAsync(id, dto);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _variantService.DeleteVariantAsync(id);
            if (!deleted) return NotFound();
            return NoContent();
        }
    }
}