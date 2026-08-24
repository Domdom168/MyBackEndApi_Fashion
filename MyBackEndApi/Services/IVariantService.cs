using MyBackEndApi.DTOs.ProductVariant;

namespace MyBackEndApi.Services
{
    public interface IVariantService
    {
        Task<IEnumerable<VariantResponseDto>> GetAllVariantsAsync();
        Task<VariantResponseDto?> GetVariantByIdAsync(int id);
        Task<IEnumerable<VariantResponseDto>> GetVariantsByProductAsync(int productId);
        Task<VariantResponseDto> CreateVariantAsync(VariantCreateDto dto);
        Task<VariantResponseDto> UpdateVariantAsync(int id, VariantUpdateDto dto);
        Task<bool> DeleteVariantAsync(int id);
    }
}
