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

namespace ClinicManagement.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IUserRepository _userRepo;
        private readonly IJwtService _jwtService;
        private readonly PasswordHasher<User> _passwordHasher = new();

        public AuthController(IUserRepository userRepo, IJwtService jwtService)
        {
            _userRepo = userRepo;
            _jwtService = jwtService;
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
            return Ok(new AuthResponse
            {
                Token = token,
                Username = user.Username,
                Role = user.RoleNavigation?.Name ?? "Guest",
                ExpiresAt = DateTime.UtcNow.AddMinutes(60)
            });
        }
    }
}
