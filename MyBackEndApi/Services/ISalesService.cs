using MyBackEndApi.DTOs.Order;

namespace MyBackEndApi.Services
{
    public interface ISalesService
    {
        Task<PagedResultDto<SaleResponseDto>> GetSalesAsync(SaleFilterDto filter);
        Task<SalesSummaryDto> GetSalesSummaryAsync(DateTime? fromDate, DateTime? toDate);
    }
}
