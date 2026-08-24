using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyBackEndApi.Data;
using MyBackEndApi.DTOs.Login;
using MyBackEndApi.Models;
using MyBackEndApi.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
namespace MyBackEndApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly AppDbContext _context;
        private readonly ITokenService _tokenService;


        public AuthController(IAuthService authService, AppDbContext context, ITokenService tokenService)
        {
            _authService = authService;
            _context =context;
            _tokenService = tokenService;
        }
        [Authorize(Roles = "admin")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetAllAdmins()
        {
            var admins = await _context.Admins
                .Select(a => new {
                    a.Id,
                    a.Name,
                    a.Email,
                    a.Phone,
                    a.Role,
                    a.IsActive,
                    a.CreatedAt,
                    a.UpdatedAt
                })
                .ToListAsync();
            return Ok(admins);
        }
        // GET: api/admins/{id}
        [Authorize(Roles = "admin,cashier")]
        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetAdminById(int id)
        {
            var admin = await _context.Admins
                .Where(a => a.Id == id)
                .Select(a => new {
                    a.Id,
                    a.Name,
                    a.Email,
                    a.Phone,
                    a.Role,
                    a.IsActive,
                    a.CreatedAt,
                    a.UpdatedAt
                })
                .FirstOrDefaultAsync();

            if (admin == null)
                return NotFound(new { message = "Admin not found" });

            return Ok(admin);
        }
        [Authorize(Roles = "admin")]
        [HttpPost]
        public async Task<IActionResult> CreateAdmin([FromBody] AdminCreateDto dto)
        {
            // Check if email already exists
            var emailExists = await _context.Admins.AnyAsync(a => a.Email == dto.Email);
            if (emailExists)
                return BadRequest(new { message = "Email already in use" });

            // Create new admin
            var admin = new Admin
            {
                Name = dto.Name,
                Email = dto.Email,
                Password = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                Phone = dto.Phone,
                Role = dto.Role,
                IsActive = dto.IsActive,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Admins.Add(admin);
            await _context.SaveChangesAsync();

            // Return the created admin (without password)
            return Ok(new
            {
                admin.Id,
                admin.Name,
                admin.Email,
                admin.Phone,
                admin.Role,
                admin.IsActive,
                admin.CreatedAt,
                admin.UpdatedAt
            });
        }
        private int GetCurrentUserId()
        {
            // Try multiple claim types
            var claim = User.FindFirst(JwtRegisteredClaimNames.Sub)
                        ?? User.FindFirst(ClaimTypes.NameIdentifier)
                        ?? User.FindFirst("userId");

            if (claim == null || !int.TryParse(claim.Value, out int id))
                throw new UnauthorizedAccessException("Invalid token: missing user ID claim");

            return id;
        }
        // PUT: api/admins/{id}
        [Authorize(Roles = "admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAdmin(int id, [FromBody] AdminUpdateDto dto)
        {
            var admin = await _context.Admins.FindAsync(id);
            if (admin == null) return NotFound();

            // Check if email already used by another admin
            var emailExists = await _context.Admins.AnyAsync(a => a.Email == dto.Email && a.Id != id);
            if (emailExists) return BadRequest(new { message = "Email already in use" });


            var currentAdminId = GetCurrentUserId(); // helper method
            bool isSelfUpdate = (currentAdminId == id);

            admin.Name = dto.Name;
            admin.Email = dto.Email;
            admin.Phone = dto.Phone;
            admin.Role = dto.Role;
            admin.IsActive = dto.IsActive;
            admin.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            // ✅ Return the updated admin (excluding password)
            // If updating another user, revoke their refresh tokens → forces logout
            if (!isSelfUpdate)
            {
                await _authService.RevokeAllUserTokens(id, Request.HttpContext.Connection.RemoteIpAddress?.ToString());
            }

            return Ok(new
            {
                admin.Id,
                admin.Name,
                admin.Email,
                admin.Phone,
                admin.Role,
                admin.IsActive,
                admin.CreatedAt,
                admin.UpdatedAt
            });
        }
        [Authorize]
        [HttpGet("debug-claims")]
        public IActionResult DebugClaims()
        {
            var claims = User.Claims.Select(c => new { c.Type, c.Value });
            return Ok(claims);
        }
        [Authorize(Roles = "admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAdmin(int id)
        {
            var admin = await _context.Admins.FindAsync(id);
            if (admin == null) return NotFound();
            _context.Admins.Remove(admin);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Admin deleted successfully" });
        }
        [Authorize]
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
        {
            var adminId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var admin = await _context.Admins.FindAsync(adminId);
            if (admin == null) return NotFound();

            if (!BCrypt.Net.BCrypt.Verify(dto.Oldpassword, admin.Password))
                return BadRequest(new { message = "Incorrect old password" });

            admin.Password = BCrypt.Net.BCrypt.HashPassword(dto.Newpassword);
            admin.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return Ok(new { message = "Password changed successfully" });
        }
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto loginDto)
        {
            var admin = await _context.Admins.FirstOrDefaultAsync(a => a.Email == loginDto.Email);
            if (admin == null || !BCrypt.Net.BCrypt.Verify(loginDto.Password, admin.Password))
                return Unauthorized();
            var accessToken = _tokenService.GenerateAccessToken(admin);
            var refreshToken = _tokenService.GenerateRefreshToken();
            SetTokenCookies(accessToken, refreshToken);

            // Save refresh token to database
            var refreshTokenEntity = new RefreshToken
            {
                AdminId = admin.Id,
                Token = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                CreatedAt = DateTime.UtcNow,
                CreatedByIp = HttpContext.Connection.RemoteIpAddress?.ToString(),
                IsRevoked = false
            };
            _context.RefreshTokens.Add(refreshTokenEntity);
            await _context.SaveChangesAsync();
            return Ok(new
            {
                admin.Id,
                admin.Name,
                admin.Email,
                admin.Phone,
                admin.Role,
                admin.IsActive,
                admin.CreatedAt,
                admin.UpdatedAt
            });
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto registerDto)
        {
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
            var result = await _authService.RegisterAsync(registerDto, ip);
            if (result == null) return BadRequest(new { message = "Email already exists" });

            SetTokenCookies(result.AccessToken, result.RefreshToken);
            return Ok(result.User);
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh()
        {
            var refreshToken = Request.Cookies["refreshToken"];
            if (string.IsNullOrEmpty(refreshToken))
                return Unauthorized(new { message = "Refresh token missing" });

            // Look up token in database
            var storedToken = await _context.RefreshTokens
                .Include(rt => rt.Admin)
                .FirstOrDefaultAsync(rt => rt.Token == refreshToken && !rt.IsRevoked && rt.ExpiresAt > DateTime.UtcNow);

            if (storedToken == null || storedToken.Admin == null)
                return Unauthorized(new { message = "Invalid or expired refresh token" });

            var admin = storedToken.Admin;

            // Revoke the old token (rotation)
            storedToken.IsRevoked = true;
            storedToken.RevokedByIp = HttpContext.Connection.RemoteIpAddress?.ToString();

            // Generate new tokens
            var newAccessToken = _tokenService.GenerateAccessToken(admin);
            var newRefreshToken = _tokenService.GenerateRefreshToken();

            // Save new refresh token
            var newRefreshTokenEntity = new RefreshToken
            {
                AdminId = admin.Id,
                Token = newRefreshToken,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                CreatedAt = DateTime.UtcNow,
                CreatedByIp = HttpContext.Connection.RemoteIpAddress?.ToString(),
                IsRevoked = false
            };
            _context.RefreshTokens.Add(newRefreshTokenEntity);
            await _context.SaveChangesAsync();

            SetTokenCookies(newAccessToken, newRefreshToken);

            return Ok(new { message = "Tokens refreshed" });
        }

        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            var adminId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            await _authService.LogoutAsync(adminId);

            Response.Cookies.Delete("accessToken");
            Response.Cookies.Delete("refreshToken");
            return Ok();
        }

        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> GetCurrentUser()
        {
            // Get user ID from the JWT claim (NameIdentifier)
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int adminId))
            {
                return Unauthorized(new { message = "Invalid token" });
            }

            var user = await _authService.GetCurrentUserAsync(adminId);
            if (user == null) return NotFound(new { message = "User not found" });
            return Ok(user);
        }

        //[HttpPost("forgot-password")]
        //public async Task<IActionResult> ForgotPassword(ForgotPasswordDto dto)
        //{
        //    await _authService.RequestPasswordResetAsync(dto.Email);
        //    return Ok(new { message = "If the email exists, a reset link has been sent." });
        //}
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var result = await _authService.RequestUserPasswordResetAsync(dto.Email);
            return Ok(new { message = "If your email is registered, you will receive a reset code." });
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var success = await _authService.ResetUserPasswordAsync(dto.Email, dto.Code, dto.NewPassword);
            if (!success)
                return BadRequest(new { message = "Invalid or expired reset code." });
            return Ok(new { message = "Password reset successfully." });
        }
        [HttpGet("debug-token/{token}")]
        public async Task<IActionResult> DebugToken(string token)
        {
            var decodedToken = Uri.UnescapeDataString(token);
            var validTokens = await _context.PasswordResetTokens
                .Where(t => t.ExpiresAt > DateTime.UtcNow)
                .ToListAsync();
            var matched = validTokens.FirstOrDefault(t => BCrypt.Net.BCrypt.Verify(decodedToken, t.Token));
            if (matched == null)
                return Ok(new { exists = false });
            return Ok(new
            {
                exists = true,
                expiresAt = matched.ExpiresAt,
                email = matched.Email
            });
        }

        //[HttpPost("reset-password")]
        //public async Task<IActionResult> ResetPassword(ResetPasswordDto dto)
        //{
        //    var result = await _authService.ResetPasswordAsync(dto.Token, dto.NewPassword);
        //    if (!result) return BadRequest(new { message = "Invalid or expired token" });
        //    return Ok(new { message = "Password reset successfully" });
        //}

        // ⚠️ ONE‑TIME MIGRATION ENDPOINT – REMOVE AFTER RUNNING
        [HttpPost("migrate-passwords")]
        public async Task<IActionResult> MigratePasswords()
        {
            var count = await _authService.MigratePlainTextPasswordsAsync();
            return Ok(new { message = $"Migrated {count} admin passwords to BCrypt." });
        }
        [HttpPut("{id}/reset-password")]
        [Authorize(Roles = "admin")] // only admin can reset passwords
        public async Task<IActionResult> ResetPassword(int id, [FromBody] AdminResetPasswordDto dto)
        {
            // Check if admin exists
            var admin = await _context.Admins.FindAsync(id);
            if (admin == null)
                return NotFound(new { message = "Admin not found." });

            // Prevent admin from resetting their own password via this endpoint? 
            // If you want to allow self-reset, you can skip this check.
            var currentAdminId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            if (currentAdminId == id)
                return BadRequest(new { message = "Use change-password endpoint for your own account." });

            // Hash new password
            admin.Password = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
            admin.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // Optionally log the action
            // await _activityLogService.LogAsync(currentAdminId, "admin", "RESET_PASSWORD", $"Reset password for admin {admin.Email}", ipAddress);

            return Ok(new { message = "Password reset successfully." });
        }



        // PUT: api/admins/profile
        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] AdminProfileUpdateDto dto)
        {
            // Get current admin ID from token
            var adminId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            if (adminId == 0)
                return Unauthorized();

            var admin = await _context.Admins.FindAsync(adminId);
            if (admin == null)
                return NotFound(new { message = "Admin not found." });

            // Check email uniqueness (if email is being changed)
            if (!string.IsNullOrEmpty(dto.Email) && dto.Email != admin.Email)
            {
                if (await _context.Admins.AnyAsync(a => a.Email == dto.Email && a.Id != adminId))
                    return BadRequest(new { message = "Email already in use." });
                admin.Email = dto.Email;
            }

            // Update fields
            if (!string.IsNullOrEmpty(dto.Name))
                admin.Name = dto.Name;
            if (!string.IsNullOrEmpty(dto.Phone))
                admin.Phone = dto.Phone;

            admin.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Profile updated successfully.",
                admin = new
                {
                    admin.Id,
                    admin.Name,
                    admin.Email,
                    admin.Phone,
                    admin.Role,
                    admin.IsActive
                }
            });
        }

        private void SetTokenCookies(string accessToken, string refreshToken)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = false,   // set to true only in production with HTTPS
                SameSite = SameSiteMode.Lax,  // or None if frontend on different domain
                Expires = DateTime.UtcNow.AddMinutes(15)
            };
            Response.Cookies.Append("accessToken", accessToken, cookieOptions);

            cookieOptions.Expires = DateTime.UtcNow.AddDays(7);
            Response.Cookies.Append("refreshToken", refreshToken, cookieOptions);
        }
    }
}
