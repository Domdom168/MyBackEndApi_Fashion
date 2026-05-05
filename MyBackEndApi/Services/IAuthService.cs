using MyBackEndApi.DTOs.Login;

namespace MyBackEndApi.Services
{
    public interface IAuthService
    {
        Task<TokenResultDto?> LoginAsync(LoginDto loginDto, string ipAddress);
        Task<TokenResultDto?> RegisterAsync(RegisterDto registerDto, string ipAddress);
        Task<TokenResultDto?> RefreshTokenAsync(string refreshToken, string ipAddress);
        Task<bool> RevokeTokenAsync(string refreshToken, string ipAddress);
        Task<bool> LogoutAsync(int adminId);
        Task<bool> RequestPasswordResetAsync(string email);
        Task<bool> ResetPasswordAsync(string token, string newPassword);
        Task<AuthResponseDto?> GetCurrentUserAsync(int adminId);
        Task<int> MigratePlainTextPasswordsAsync();  // one-time migration
        Task RevokeAllUserTokens(int adminId, string revokedByIp);

    }
}
