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
        private readonly IEmailSender _emailService;

        public AuthService(AppDbContext context, ITokenService tokenService,IEmailSender emailService)
        {
            _context = context;
            _tokenService = tokenService;
            _emailService = emailService;
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

        public async Task<bool> RequestUserPasswordResetAsync(string email)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null) return false;

            // Remove old tokens
            var old = await _context.PasswordResetTokens.Where(t => t.Email == email).ToListAsync();
            if (old.Any()) _context.PasswordResetTokens.RemoveRange(old);

            // Generate 6‑digit numeric code
            var plainCode = new Random().Next(100000, 999999).ToString();
            var hashedCode = BCrypt.Net.BCrypt.HashPassword(plainCode);

            var token = new PasswordResetToken
            {
                Email = email,
                Token = hashedCode,
                ExpiresAt = DateTime.UtcNow.AddMinutes(10), // short expiry
                CreatedAt = DateTime.UtcNow
            };
            _context.PasswordResetTokens.Add(token);
            await _context.SaveChangesAsync();

            // Send email with the plain code
            await _emailService.SendEmailAsync(
                email,
                "Password Reset Code",
                $"<p>Your password reset code is: <strong>{plainCode}</strong></p><p>This code expires in 10 minutes.</p>"
            );
            return true;
        }

        public async Task<bool> ResetUserPasswordAsync(string email, string code, string newPassword)
        {
            var valid = await _context.PasswordResetTokens
                .Where(t => t.Email == email && t.ExpiresAt > DateTime.UtcNow)
                .ToListAsync();
            var match = valid.FirstOrDefault(t => BCrypt.Net.BCrypt.Verify(code, t.Token));
            if (match == null) return false;

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null) return false;

            user.Password = BCrypt.Net.BCrypt.HashPassword(newPassword);
            user.UpdatedAt = DateTime.UtcNow;
            _context.PasswordResetTokens.Remove(match);
            await _context.SaveChangesAsync();
            return true;
        }
        // Placeholder for email sending – replace with real email service
        //private async Task SendResetEmail(string email, string token)
        //{
        //    // In production, use SendGrid, SMTP, etc.
        //    // For testing, we'll just log it
        //    var resetLink = $"https://yourfrontend.com/admin/reset-password?token={Uri.EscapeDataString(token)}";
        //    Console.WriteLine($"Password reset link for {email}: {resetLink}");
        //    // In a real app, send email with the link
        //    await Task.CompletedTask;
        //}

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
