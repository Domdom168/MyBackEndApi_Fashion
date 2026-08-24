using Microsoft.EntityFrameworkCore;
using MyBackEndApi.Data;
using MyBackEndApi.DTOs.CustomerLogin;
using MyBackEndApi.DTOs.Login;
using MyBackEndApi.Models;
namespace MyBackEndApi.Services
{
    public class CustomerService : ICustomerService
    {
        private readonly AppDbContext _context;
        private readonly IEmailSender _emailService;
        public CustomerService(AppDbContext context,IEmailSender emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        public async Task<User?> UpdateProfileAsync(int userId, UpdateProfileDto dto)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return null;
            if (!string.IsNullOrEmpty(dto.Name)) user.Name = dto.Name;
            if (!string.IsNullOrEmpty(dto.Phone)) user.Phone = dto.Phone;
            user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<bool> ChangePasswordAsync(int userId, ChangePasswordDtos dto)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return false;
            if (!BCrypt.Net.BCrypt.Verify(dto.OldPassword, user.Password))
                return false;
            user.Password = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
            user.UpdatedAt = DateTime.UtcNow;
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

        public async Task<UserResponseDto?> GetUserByIdAsync(int id)
        {
            var user = await _context.Users
                .Where(u => u.Id == id)
                .Select(u => new UserResponseDto
                {
                    Id = u.Id,
                    Name = u.Name,
                    Email = u.Email,
                    Phone = u.Phone,
                    Role = u.Role,
                    CreatedAt = u.CreatedAt,
                    UpdatedAt = u.UpdatedAt
                })
                .FirstOrDefaultAsync();
            return user;
        }

    }
}
