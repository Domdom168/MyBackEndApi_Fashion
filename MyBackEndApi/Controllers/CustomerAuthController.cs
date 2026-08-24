using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyBackEndApi.Data;
using MyBackEndApi.DTOs.CustomerLogin;
using MyBackEndApi.DTOs.Login;
using MyBackEndApi.Models;
using MyBackEndApi.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
namespace MyBackEndApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomerAuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ITokenService _tokenService; // reuse same token service
        private readonly ICustomerService _authService;
        public CustomerAuthController(AppDbContext context, ITokenService tokenService,ICustomerService authService)
        {
            _context = context;
            _tokenService = tokenService;
            _authService = authService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(CustomerLoginDto dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
            if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.Password))
                return Unauthorized();

            var token = _tokenService.GenerateTokenForUser(user);   // ← generate token
            return Ok(new { token, user = new { user.Id, user.Name, user.Email, user.Role } });
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(CustomerRegisterDto dto)
        {
            if (await _context.Users.AnyAsync(u => u.Email == dto.Email))
                return BadRequest(new { message = "Email already exists" });

            var user = new User
            {
                Name = dto.Name,
                Email = dto.Email,
                Password = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                Phone = dto.Phone,
                Role = "user",
                //IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var token = _tokenService.GenerateTokenForUser(user);
            return Ok(new CustomerAuthResponseDto
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                Role = user.Role,
                Token = token
            });
        }

        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> GetCurrentUser()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst(JwtRegisteredClaimNames.Sub);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                return Unauthorized();

            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound();

            return Ok(new
            {
                user.Id,
                user.Name,
                user.Email,
                user.Role,
                user.Phone
            });
        }

        private int GetCurrentUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst(JwtRegisteredClaimNames.Sub);
            if (claim == null || !int.TryParse(claim.Value, out int userId))
                throw new UnauthorizedAccessException("Invalid token");
            return userId;
        }

        [Authorize]
        [HttpGet("{id}")]
        public async Task<ActionResult<UserResponseDto>> GetUserById(int id)
        {
            var currentUserId = GetCurrentUserId();
            var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;

            // Allow if user is fetching their own profile OR is admin
            if (currentUserId != id && currentUserRole != "admin")
                return Forbid();

            var user = await _authService.GetUserByIdAsync(id);
            if (user == null)
                return NotFound();
            return Ok(user);
        }

        [Authorize]
        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto dto)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var user = await _authService.UpdateProfileAsync(userId, dto);
            if (user == null) return NotFound();
            return Ok(new { user.Id, user.Name, user.Email, user.Phone, user.Role });
        }

        [Authorize]
        [HttpPut("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDtos dto)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var success = await _authService.ChangePasswordAsync(userId, dto);
            if (!success) return BadRequest(new { message = "Current password is incorrect." });
            return Ok(new { message = "Password changed successfully." });
        }

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
    }
    }
