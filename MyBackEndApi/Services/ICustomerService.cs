using MyBackEndApi.DTOs.CustomerLogin;
using MyBackEndApi.DTOs.Login;
using MyBackEndApi.Models;

namespace MyBackEndApi.Services
{
    public interface ICustomerService
    {
        Task<User?> UpdateProfileAsync(int userId, UpdateProfileDto dto);
        Task<bool> ChangePasswordAsync(int userId, ChangePasswordDtos dto);
        Task<bool> RequestUserPasswordResetAsync(string email);
        Task<bool> ResetUserPasswordAsync(string token,string code ,string newPassword);
        Task<UserResponseDto?> GetUserByIdAsync(int id);
    }
}
