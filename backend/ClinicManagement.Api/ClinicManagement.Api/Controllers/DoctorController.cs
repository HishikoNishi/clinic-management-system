using ClinicManagement.Api.Data;
using ClinicManagement.Api.Dtos.Doctor;
using ClinicManagement.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class DoctorController : ControllerBase
    {
        private readonly ClinicDbContext _context;
        private readonly PasswordHasher<User> _passwordHasher;

        public DoctorController(ClinicDbContext context)
        {
            _context = context;
            _passwordHasher = new PasswordHasher<User>();
        }

        /* ================= GET ALL ================= */
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var doctors = await _context.Doctors
                .Include(d => d.User)
                .Select(d => new DoctorDto
                {
                    Id = d.Id,
                    Code = d.Code,
                    Specialty = d.Specialty,
                    LicenseNumber = d.LicenseNumber,
                    Username = d.User.Username,
                    Status = d.Status.ToString()
                })
                .ToListAsync();

            return Ok(doctors);
        }

        /* ================= GET BY ID ================= */
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> Get(Guid id)
        {
            var doctor = await _context.Doctors
                .Include(d => d.User)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (doctor == null)
                return NotFound();

            return Ok(new DoctorDto
            {
                Id = doctor.Id,
                Code = doctor.Code,
                Specialty = doctor.Specialty,
                LicenseNumber = doctor.LicenseNumber,
                Username = doctor.User.Username,
                Status = doctor.Status.ToString()
            });
        }

        /* ================= CREATE ================= */
        [HttpPost]
        public async Task<IActionResult> Create(CreateDoctorDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (await _context.Users.AnyAsync(u => u.Username == dto.Username))
                return BadRequest("Username already exists.");

            if (await _context.Doctors.AnyAsync(d => d.Code == dto.Code))
                return BadRequest("Doctor code already exists.");

            var doctorRole = await _context.Roles
                .FirstOrDefaultAsync(r => r.Name == "Doctor");

            if (doctorRole == null)
                return BadRequest("Doctor role not found.");

            var user = new User
            {
                Id = Guid.NewGuid(),
                Username = dto.Username,
                RoleId = doctorRole.Id,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            user.PasswordHash = _passwordHasher.HashPassword(user, dto.Password);

            var doctor = new Doctor
            {
                Id = Guid.NewGuid(),
                Code = dto.Code,
                Specialty = dto.Specialty,
                LicenseNumber = dto.LicenseNumber,
                Status = DoctorStatus.Active,
                UserId = user.Id,
                CreatedAt = DateTime.UtcNow
            };

            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                await _context.Users.AddAsync(user);
                await _context.Doctors.AddAsync(doctor);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }

            return CreatedAtAction(nameof(Get), new { id = doctor.Id }, new
            {
                message = "Doctor created successfully",
                doctorId = doctor.Id
            });
        }

        /* ================= UPDATE ================= */
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, UpdateDoctorDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var doctor = await _context.Doctors
                .Include(d => d.User)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (doctor == null)
                return NotFound();

            if (doctor.Code != dto.Code &&
                await _context.Doctors.AnyAsync(d => d.Code == dto.Code))
                return BadRequest("Doctor code already exists.");

            if (doctor.User.Username != dto.Username &&
                await _context.Users.AnyAsync(u => u.Username == dto.Username))
                return BadRequest("Username already exists.");

            doctor.Code = dto.Code;
            doctor.Specialty = dto.Specialty;
            doctor.LicenseNumber = dto.LicenseNumber;
            doctor.User.Username = dto.Username;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Doctor updated successfully" });
        }

        /* ================= DELETE ================= */
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var doctor = await _context.Doctors
                .Include(d => d.User)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (doctor == null)
                return NotFound();

            _context.Doctors.Remove(doctor);
            _context.Users.Remove(doctor.User);

            await _context.SaveChangesAsync();

            return Ok(new { message = "Doctor deleted successfully" });
        }
    }
}