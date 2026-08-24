using MyBackEndApi.DTOs.Banner;

namespace MyBackEndApi.Services
{
    public interface IBannerService
    {
        Task<IEnumerable<BannerResponseDto>> GetAllBannersAsync();
        Task<BannerResponseDto?> GetBannerByIdAsync(int id);
        Task<BannerResponseDto> CreateBannerAsync(BannerCreateDto dto);
        Task<BannerResponseDto?> UpdateBannerAsync(int id, BannerUpdateDto dto);
        Task<bool> DeleteBannerAsync(int id);
        Task<IEnumerable<BannerResponseDto>> GetActiveBannersAsync();
    }
}
