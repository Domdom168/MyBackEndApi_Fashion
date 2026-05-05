using MyBackEndApi.DTOs.Order;

namespace MyBackEndApi.Services
{
    public interface IOrderService
    {
        Task<PagedResultDto<OrderResponseDto>> GetOrdersAsync(OrderFilterDto filter);
        Task<OrderResponseDto?> GetOrderByIdAsync(int id);
        Task<bool> UpdateOrderStatusAsync(int id, string newStatus, int currentAdminId);
        Task<bool> DeleteOrderAsync(int id);
    }
}
