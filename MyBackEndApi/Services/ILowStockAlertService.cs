using MyBackEndApi.DTOs.LowStockAlert;

namespace MyBackEndApi.Services
{
    public interface ILowStockAlertService
    {
        Task<List<LowStockAlertDto>> GetAllAlertsAsync();
        Task<LowStockAlertDto?> GetAlertByProductIdAsync(int productId);
        Task<LowStockAlertDto> CreateOrUpdateAlertAsync(int productId, int threshold);
        Task<bool> DeleteAlertAsync(int id);
    }
}
