using MyBackEndApi.DTOs.Order;
using MyBackEndApi.Models;

namespace MyBackEndApi.Services
{
    public interface IOrderService
    {
        Task<PagedResultDto<OrderResponseDto>> GetOrdersAsync(OrderFilterDto filter);
        Task<OrderResponseDto?> GetOrderByIdAsync(int id);
        Task<Order> CreateOrderAsync(OrderCreateDto dto, int? userId, int? cashierId);
        Task<bool> UpdateOrderStatusAsync(int id, string newStatus, int currentAdminId);
        Task<bool> DeleteOrderAsync(int id);
        Task<User?> GetUserByIdAsync(int id);
    }
}
