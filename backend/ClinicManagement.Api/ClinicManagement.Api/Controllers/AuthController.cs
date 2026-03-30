using System;
using System.Threading.Tasks;
using ClinicManagement.Api.Data;
using ClinicManagement.Api.Dtos.User;
using ClinicManagement.Api.Models;
using ClinicManagement.Api.Repositories;
using ClinicManagement.Api.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

namespace ClinicManagement.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IUserRepository _userRepo;
        private readonly IJwtService _jwtService;
        private readonly ClinicDbContext _context;
        private readonly int _refreshDays;
        private readonly PasswordHasher<User> _passwordHasher = new();

        public AuthController(IUserRepository userRepo, IJwtService jwtService, ClinicDbContext context, IConfiguration configuration)
        {
            _userRepo = userRepo;
            _jwtService = jwtService;
            _context = context;
            _refreshDays = int.TryParse(configuration["Jwt:RefreshDays"], out var days) ? days : 7;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var user = await _userRepo.GetByUsernameAsync(request.Username);
            if (user == null || !user.IsActive)
            {
                return Unauthorized(new { message = "Invalid credentials." });
            }

            var verify = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
            if (verify == PasswordVerificationResult.Failed)
            {
                return Unauthorized(new { message = "Invalid credentials." });
            }

            var token = _jwtService.GenerateToken(user);
            var refresh = new RefreshToken
            {
                UserId = user.Id,
                Token = GenerateRefreshToken(),
                ExpiresAt = DateTime.UtcNow.AddDays(_refreshDays)
            };
            _context.RefreshTokens.Add(refresh);
            await _context.SaveChangesAsync();

            return Ok(new AuthResponse
            {
                Token = token,
                Username = user.Username,
                Role = user.RoleNavigation?.Name ?? "Guest",
                ExpiresAt = DateTime.UtcNow.AddMinutes(60),
                RefreshToken = refresh.Token,
                RefreshExpiresAt = refresh.ExpiresAt
            });
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] RefreshRequest request)
        {
            var stored = await _context.RefreshTokens
                .Include(r => r.User)
                .ThenInclude(u => u.RoleNavigation)
                .FirstOrDefaultAsync(r => r.Token == request.RefreshToken);

            if (stored == null || stored.IsRevoked || stored.ExpiresAt <= DateTime.UtcNow)
                return Unauthorized(new { message = "Refresh token không hợp lệ hoặc đã hết hạn" });

            stored.IsRevoked = true;

            var user = stored.User;
            if (user == null)
            {
                user = await _context.Users.Include(u => u.RoleNavigation).FirstOrDefaultAsync(u => u.Id == stored.UserId);
                if (user == null) return Unauthorized(new { message = "User không tồn tại" });
            }

            var newAccess = _jwtService.GenerateToken(user);
            var newRefresh = new RefreshToken
            {
                UserId = user.Id,
                Token = GenerateRefreshToken(),
                ExpiresAt = DateTime.UtcNow.AddDays(_refreshDays)
            };

            _context.RefreshTokens.Add(newRefresh);
            await _context.SaveChangesAsync();

            return Ok(new AuthResponse
            {
                Token = newAccess,
                Username = user.Username,
                Role = user.RoleNavigation?.Name ?? "Guest",
                ExpiresAt = DateTime.UtcNow.AddMinutes(60),
                RefreshToken = newRefresh.Token,
                RefreshExpiresAt = newRefresh.ExpiresAt
            });
        }

        private string GenerateRefreshToken()
        {
            var bytes = new byte[32];
            RandomNumberGenerator.Fill(bytes);
            return Convert.ToBase64String(bytes);
        }
    }
}
