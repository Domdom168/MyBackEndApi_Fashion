using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyBackEndApi.Data;
using MyBackEndApi.DTOs.CustomerLogin;
using MyBackEndApi.Models;
using MyBackEndApi.Services;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
namespace MyBackEndApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomerAuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ITokenService _tokenService; // reuse same token service

        public CustomerAuthController(AppDbContext context, ITokenService tokenService)
        {
            _context = context;
            _tokenService = tokenService;
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


    }
    }
