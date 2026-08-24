using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyBackEndApi.DTOs.Banner;
using MyBackEndApi.Services;

namespace MyBackEndApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BannersController : ControllerBase
    {
        private readonly IBannerService _bannerService;

        public BannersController(IBannerService bannerService)
        {
            _bannerService = bannerService;
        }

        // PUBLIC: get active banners (for homepage)
        [HttpGet("active")]
        public async Task<ActionResult<IEnumerable<BannerResponseDto>>> GetActiveBanners()
        {
            var banners = await _bannerService.GetActiveBannersAsync();
            return Ok(banners);
        }

        // ADMIN: get all banners
        [HttpGet]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult<IEnumerable<BannerResponseDto>>> GetAllBanners()
        {
            var banners = await _bannerService.GetAllBannersAsync();
            return Ok(banners);
        }

        // ADMIN: get single banner
        [HttpGet("{id}")]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult<BannerResponseDto>> GetBannerById(int id)
        {
            var banner = await _bannerService.GetBannerByIdAsync(id);
            if (banner == null) return NotFound();
            return Ok(banner);
        }

        // ADMIN: create banner
        [HttpPost]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult<BannerResponseDto>> CreateBanner([FromForm] BannerCreateDto dto)
        {
            try
            {
                var banner = await _bannerService.CreateBannerAsync(dto);
                return CreatedAtAction(nameof(GetBannerById), new { id = banner.Id }, banner);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // ADMIN: update banner
        [HttpPut("{id}")]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult<BannerResponseDto>> UpdateBanner(int id, [FromForm] BannerUpdateDto dto)
        {
            try
            {
                var banner = await _bannerService.UpdateBannerAsync(id, dto);
                if (banner == null) return NotFound();
                return Ok(banner);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // ADMIN: delete banner
        [HttpDelete("{id}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> DeleteBanner(int id)
        {
            var deleted = await _bannerService.DeleteBannerAsync(id);
            if (!deleted) return NotFound();
            return NoContent();
        }
    }
}