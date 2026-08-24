using MyBackEndApi.DTOs.Cart;

namespace MyBackEndApi.Services
{
    
        public interface ICartService
        {
            Task<IEnumerable<CartItemDto>> GetCartAsync(int userId);
            Task AddToCartAsync(int userId, AddToCartDto dto);
            Task UpdateCartItemAsync(int userId, int cartItemId, int quantity);
            Task RemoveCartItemAsync(int userId, int cartItemId);
            Task ClearCartAsync(int userId);
        }
    
}
