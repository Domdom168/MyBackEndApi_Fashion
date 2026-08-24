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
        Task<bool> RequestUserPasswordResetAsync(string email);
        Task<bool> ResetUserPasswordAsync(string email, string code, string newPassword);
        Task<AuthResponseDto?> GetCurrentUserAsync(int adminId);
        Task<int> MigratePlainTextPasswordsAsync();  // one-time migration
        Task RevokeAllUserTokens(int adminId, string revokedByIp);

    }
}
