using MyBackEndApi.Data;
using MyBackEndApi.DTOs.Banner;
using MyBackEndApi.Helpers;
using MyBackEndApi.Models;
using Microsoft.EntityFrameworkCore;
namespace MyBackEndApi.Services
{
    public class BannerService : IBannerService
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;

        public BannerService(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        public async Task<IEnumerable<BannerResponseDto>> GetAllBannersAsync()
        {
            return await _context.Banners
                .OrderBy(b => b.DisplayOrder)
                .Select(b => MapToDto(b))
                .ToListAsync();
        }

        public async Task<BannerResponseDto?> GetBannerByIdAsync(int id)
        {
            var banner = await _context.Banners.FindAsync(id);
            return banner == null ? null : MapToDto(banner);
        }

        public async Task<BannerResponseDto> CreateBannerAsync(BannerCreateDto dto)
        {
            if (dto.ImageFile == null)
                throw new ArgumentException("Image file is required.");

            var banner = new Banner
            {
                Title = dto.Title,
                Link = dto.Link,
                DisplayOrder = dto.DisplayOrder,
                IsActive = dto.IsActive,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var imageUrl = await FileHelper.SaveBannerImageAsync(dto.ImageFile, _env);
            banner.ImageUrl = imageUrl;

            _context.Banners.Add(banner);
            await _context.SaveChangesAsync();
            return MapToDto(banner);
        }

        public async Task<BannerResponseDto?> UpdateBannerAsync(int id, BannerUpdateDto dto)
        {
            var banner = await _context.Banners.FindAsync(id);
            if (banner == null) return null;

            banner.Title = dto.Title;
            banner.Link = dto.Link;
            banner.DisplayOrder = dto.DisplayOrder;
            banner.IsActive = dto.IsActive;
            banner.UpdatedAt = DateTime.UtcNow;

            if (dto.ImageFile != null)
            {
                if (!string.IsNullOrEmpty(banner.ImageUrl))
                    FileHelper.DeleteBannerImage(banner.ImageUrl, _env);

                var newImageUrl = await FileHelper.SaveBannerImageAsync(dto.ImageFile, _env);
                banner.ImageUrl = newImageUrl;
            }

            await _context.SaveChangesAsync();
            return MapToDto(banner);
        }

        public async Task<bool> DeleteBannerAsync(int id)
        {
            var banner = await _context.Banners.FindAsync(id);
            if (banner == null) return false;

            if (!string.IsNullOrEmpty(banner.ImageUrl))
                FileHelper.DeleteBannerImage(banner.ImageUrl, _env);

            _context.Banners.Remove(banner);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<BannerResponseDto>> GetActiveBannersAsync()
        {
            return await _context.Banners
                .Where(b => b.IsActive)
                .OrderBy(b => b.DisplayOrder)
                .Select(b => MapToDto(b))
                .ToListAsync();
        }

        private static BannerResponseDto MapToDto(Banner b) => new()
        {
            Id = b.Id,
            Title = b.Title,
            ImageUrl = b.ImageUrl,
            Link = b.Link,
            DisplayOrder = b.DisplayOrder,
            IsActive = b.IsActive,
            CreatedAt = b.CreatedAt,
            UpdatedAt = b.UpdatedAt
        };
    }
}
