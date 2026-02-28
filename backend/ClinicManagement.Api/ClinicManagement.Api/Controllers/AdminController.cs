using ClinicManagement.Api.Data;
using ClinicManagement.Api.Dtos.User;
using ClinicManagement.Api.Models;
using ClinicManagement.Api.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ClinicManagement.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class AdminController : ControllerBase
    {
        private readonly IUserRepository _userRepo;
        private readonly ClinicDbContext _context;
        private readonly PasswordHasher<User> _passwordHasher = new();

        public AdminController(IUserRepository userRepo, ClinicDbContext context)
        {
            _userRepo = userRepo;
            _context = context;
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _userRepo.GetAllAsync();
            var response = new List<UserDto>();

            foreach (var user in users)
            {
                response.Add(new UserDto
                {
                    Id = user.Id,
                    Username = user.Username,
                    Role = user.RoleNavigation?.Name ?? "User"
                });
            }

            return Ok(response);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserById(Guid id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!User.IsInRole("Admin") && userId != id.ToString())
            {
                return Forbid();
            }

            var user = await _userRepo.GetByIdAsync(id);
            if (user == null)
            {
                return NotFound(new { message = "User not found." });
            }

            return Ok(new UserDto
            {
                Id = user.Id,
                Username = user.Username,
                Role = user.RoleNavigation?.Name ?? "User"
            });
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] CreateAccountRequest request)
        {
            var existing = await _userRepo.GetByUsernameAsync(request.Username);
            if (existing != null)
            {
                return Conflict(new { message = "Username already exists." });
            }

            var role = await _context.Roles.FirstOrDefaultAsync(r => r.Name == request.Role);
            if (role == null)
            {
                return BadRequest(new { message = $"Role '{request.Role}' not found." });
            }

            var user = new User
            {
                Username = request.Username.ToLowerInvariant(),
                RoleId = role.Id
            };

            user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);
            await _userRepo.AddAsync(user);

            user = await _userRepo.GetByIdAsync(user.Id);

            return CreatedAtAction(nameof(GetUserById), new { id = user.Id }, new UserDto
            {
                Id = user.Id,
                Username = user.Username,
                Role = user.RoleNavigation?.Name ?? "User"
            });
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("{id}/roles/{roleName}")]
        public async Task<IActionResult> AssignRoleToUser(Guid id, string roleName)
        {
            var user = await _userRepo.GetByIdAsync(id);
            if (user == null)
            {
                return NotFound(new { message = "User not found." });
            }

            var role = await _context.Roles.FirstOrDefaultAsync(r => r.Name == roleName);
            if (role == null)
            {
                return BadRequest(new { message = $"Role '{roleName}' not found." });
            }

            user.RoleId = role.Id;
            await _userRepo.UpdateAsync(user);

            user = await _userRepo.GetByIdAsync(user.Id);

            return Ok(new UserDto
            {
                Id = user.Id,
                Username = user.Username,
                Role = user.RoleNavigation?.Name ?? "User"
            });
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(Guid id)
        {
            var user = await _userRepo.GetByIdAsync(id);
            if (user == null)
            {
                return NotFound(new { message = "User not found." });
            }

            await _userRepo.DeleteAsync(id);
            return NoContent();
        }
    }
}
