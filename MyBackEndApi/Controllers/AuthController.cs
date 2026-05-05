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
            //var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
            //var result = await _authService.LoginAsync(loginDto, ip);
            //if (result == null) return Unauthorized(new { message = "Invalid email or password" });

            //SetTokenCookies(result.AccessToken, result.RefreshToken);
            //return Ok(result.User);
            var admin = await _context.Admins.FirstOrDefaultAsync(a => a.Email == loginDto.Email);
            if (admin == null || !BCrypt.Net.BCrypt.Verify(loginDto.Password, admin.Password))
                return Unauthorized();
            var accessToken = _tokenService.GenerateAccessToken(admin);
            var refreshToken = _tokenService.GenerateRefreshToken();
            SetTokenCookies(accessToken, refreshToken);

            // ✅ Return all fields the frontend needs (including phone and isActive)
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
            if (string.IsNullOrEmpty(refreshToken)) return Unauthorized();

            var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
            var result = await _authService.RefreshTokenAsync(refreshToken, ip);
            if (result == null) return Unauthorized();

            SetTokenCookies(result.AccessToken, result.RefreshToken);
            return Ok(result.User);
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

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordDto dto)
        {
            await _authService.RequestPasswordResetAsync(dto.Email);
            return Ok(new { message = "If the email exists, a reset link has been sent." });
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword(ResetPasswordDto dto)
        {
            var result = await _authService.ResetPasswordAsync(dto.Token, dto.NewPassword);
            if (!result) return BadRequest(new { message = "Invalid or expired token" });
            return Ok(new { message = "Password reset successfully" });
        }

        // ⚠️ ONE‑TIME MIGRATION ENDPOINT – REMOVE AFTER RUNNING
        [HttpPost("migrate-passwords")]
        public async Task<IActionResult> MigratePasswords()
        {
            var count = await _authService.MigratePlainTextPasswordsAsync();
            return Ok(new { message = $"Migrated {count} admin passwords to BCrypt." });
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
