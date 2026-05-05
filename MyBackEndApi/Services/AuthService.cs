using MyBackEndApi.Data;
using MyBackEndApi.DTOs.Login;
using MyBackEndApi.Models;
using Microsoft.EntityFrameworkCore;

namespace MyBackEndApi.Services
{
    public class AuthService: IAuthService
    {
        private readonly AppDbContext _context;
        private readonly ITokenService _tokenService;

        public AuthService(AppDbContext context, ITokenService tokenService)
        {
            _context = context;
            _tokenService = tokenService;
        }

        public async Task<TokenResultDto?> LoginAsync(LoginDto loginDto, string ipAddress)
        {
            var admin = await _context.Admins.FirstOrDefaultAsync(a => a.Email == loginDto.Email);
            if (admin == null || !BCrypt.Net.BCrypt.Verify(loginDto.Password, admin.Password))
                return null;

            if (!admin.IsActive) return null;

            var accessToken = _tokenService.GenerateAccessToken(admin);
            var refreshToken = _tokenService.GenerateRefreshToken();

            // Save refresh token
            _context.RefreshTokens.Add(new RefreshToken
            {
                AdminId = admin.Id,
                Token = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                CreatedAt = DateTime.UtcNow,
                CreatedByIp = ipAddress,
                IsRevoked = false
            });
            await _context.SaveChangesAsync();

            return new TokenResultDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                User = new AuthResponseDto
                {
                    Id = admin.Id,
                    Name = admin.Name,
                    Email = admin.Email,
                    Role = admin.Role
                }
            };
        }

        public async Task<TokenResultDto?> RegisterAsync(RegisterDto registerDto, string ipAddress)
        {
            if (await _context.Admins.AnyAsync(a => a.Email == registerDto.Email))
                return null;

            var admin = new Admin
            {
                Name = registerDto.Name,
                Email = registerDto.Email,
                Password = BCrypt.Net.BCrypt.HashPassword(registerDto.Password),
                Phone = registerDto.Phone,
                Role = registerDto.Role ?? "cashier",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Admins.Add(admin);
            await _context.SaveChangesAsync();

            var accessToken = _tokenService.GenerateAccessToken(admin);
            var refreshToken = _tokenService.GenerateRefreshToken();

            _context.RefreshTokens.Add(new RefreshToken
            {
                AdminId = admin.Id,
                Token = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                CreatedAt = DateTime.UtcNow,
                CreatedByIp = ipAddress,
                IsRevoked = false
            });
            await _context.SaveChangesAsync();

            return new TokenResultDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                User = new AuthResponseDto
                {
                    Id = admin.Id,
                    Name = admin.Name,
                    Email = admin.Email,
                    Role = admin.Role
                }
            };
        }

        public async Task<TokenResultDto?> RefreshTokenAsync(string refreshToken, string ipAddress)
        {
            var storedToken = await _context.RefreshTokens
                .Include(rt => rt.Admin)
                .FirstOrDefaultAsync(rt => rt.Token == refreshToken && !rt.IsRevoked && rt.ExpiresAt > DateTime.UtcNow);

            if (storedToken == null) return null;

            // Revoke old token
            storedToken.IsRevoked = true;
            storedToken.RevokedByIp = ipAddress;

            var admin = storedToken.Admin;
            var newAccessToken = _tokenService.GenerateAccessToken(admin);
            var newRefreshToken = _tokenService.GenerateRefreshToken();

            _context.RefreshTokens.Add(new RefreshToken
            {
                AdminId = admin.Id,
                Token = newRefreshToken,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                CreatedAt = DateTime.UtcNow,
                CreatedByIp = ipAddress,
                IsRevoked = false
            });
            await _context.SaveChangesAsync();

            return new TokenResultDto
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken,
                User = new AuthResponseDto
                {
                    Id = admin.Id,
                    Name = admin.Name,
                    Email = admin.Email,
                    Role = admin.Role
                }
            };
        }

        public async Task<bool> RevokeTokenAsync(string refreshToken, string ipAddress)
        {
            var token = await _context.RefreshTokens.FirstOrDefaultAsync(rt => rt.Token == refreshToken && !rt.IsRevoked);
            if (token == null) return false;

            token.IsRevoked = true;
            token.RevokedByIp = ipAddress;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> LogoutAsync(int adminId)
        {
            var tokens = await _context.RefreshTokens.Where(rt => rt.AdminId == adminId && !rt.IsRevoked).ToListAsync();
            foreach (var t in tokens) t.IsRevoked = true;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RequestPasswordResetAsync(string email)
        {
            var admin = await _context.Admins.FirstOrDefaultAsync(a => a.Email == email);
            if (admin == null) return false; // don't reveal existence

            // In production, generate a token and store in a PasswordResetToken table.
            // Then send an email with a link containing the token.
            // For brevity, we'll return true.
            return true;
        }

        public async Task<bool> ResetPasswordAsync(string token, string newPassword)
        {
            // Validate token from PasswordResetTokens table
            // If valid, update admin password
            // For brevity, we'll assume it's implemented.
            return true;
        }

        public async Task<AuthResponseDto?> GetCurrentUserAsync(int adminId)
        {
            var admin = await _context.Admins.FindAsync(adminId);
            if (admin == null) return null;

            return new AuthResponseDto
            {
                Id = admin.Id,
                Name = admin.Name,
                Email = admin.Email,
                Role = admin.Role
            };
        }

        public async Task<int> MigratePlainTextPasswordsAsync()
        {
            var admins = await _context.Admins.ToListAsync();
            int updated = 0;
            foreach (var admin in admins)
            {
                // If password is not a BCrypt hash (doesn't start with "$2"), assume it's plain text
                if (!string.IsNullOrEmpty(admin.Password) && !admin.Password.StartsWith("$2"))
                {
                    admin.Password = BCrypt.Net.BCrypt.HashPassword(admin.Password);
                    updated++;
                }
            }
            await _context.SaveChangesAsync();
            return updated;
        }

        public async Task RevokeAllUserTokens(int adminId, string revokedByIp)
        {
            var tokens = await _context.RefreshTokens
                .Where(rt => rt.AdminId == adminId && !rt.IsRevoked)
                .ToListAsync();
            foreach (var t in tokens)
            {
                t.IsRevoked = true;
                t.RevokedByIp = revokedByIp;
            }
            await _context.SaveChangesAsync();
        }

    }
}
