using MyBackEndApi.Models;

namespace MyBackEndApi.Services
{
    public interface ITokenService
    {
        string GenerateAccessToken(Admin admin);
        string GenerateTokenForUser(User user);
        string GenerateRefreshToken();
    }
}
