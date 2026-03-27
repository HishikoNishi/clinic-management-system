using ClinicManagement.Api.Data;
using ClinicManagement.Api.Dtos.User;
using ClinicManagement.Api.Models;
using ClinicManagement.Api.Repositories;
using ClinicManagement.Api.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IUserRepository _userRepo;
    private readonly IJwtService _jwtService;
    private readonly ClinicDbContext _context;
    private readonly PasswordHasher<User> _passwordHasher = new();

    public AuthController(IUserRepository userRepo, IJwtService jwtService, ClinicDbContext context)
    {
        _userRepo = userRepo;
        _jwtService = jwtService;
        _context = context;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var user = await _userRepo.GetByUsernameAsync(request.Username);
        if (user == null || !user.IsActive)
            return Unauthorized(new { message = "Invalid credentials." });

        var verify = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
        if (verify == PasswordVerificationResult.Failed)
            return Unauthorized(new { message = "Invalid credentials." });

        var token = _jwtService.GenerateToken(user);

        // lấy doctorId nếu user là bác sĩ
        Guid? doctorId = null;
        var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.UserId == user.Id);
        if (doctor != null)
            doctorId = doctor.Id;

        return Ok(new
        {
            Token = token,
            Username = user.Username,
            Role = user.RoleNavigation?.Name ?? "Guest",
            ExpiresAt = DateTime.UtcNow.AddMinutes(60),
            DoctorId = doctorId
        });
    }
}
