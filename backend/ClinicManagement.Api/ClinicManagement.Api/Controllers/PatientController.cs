using ClinicManagement.Api.Data;
using ClinicManagement.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ClinicManagement.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Bắt buộc phải đăng nhập
    public class PatientsController : ControllerBase
    {
        private readonly ClinicDbContext _context;

        public PatientsController(ClinicDbContext context)
        {
            _context = context;
        }

        // ============================================================
        // GET: /api/patients
        // Admin, Staff: xem tất cả bệnh nhân
        // ============================================================
        [HttpGet]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> GetAllPatients()
        {
            var patients = await _context.Patients
                .AsNoTracking()
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            return Ok(patients);
        }

        // ============================================================
        // GET: /api/patients/{id}
        // Admin, Staff, Doctor: xem chi tiết bệnh nhân
        // Doctor chỉ được xem bệnh nhân có lịch khám với mình
        // ============================================================
        [HttpGet("{id:guid}")]
        [Authorize(Roles = "Admin,Staff,Doctor")]
        public async Task<IActionResult> GetPatientById(Guid id)
        {
            var patient = await _context.Patients
                .Include(p => p.Appointments)
                .ThenInclude(a => a.Doctor)
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == id);

            if (patient == null)
                return NotFound(new { message = "Patient not found." });

            // Nếu là Doctor -> chỉ xem bệnh nhân có lịch với mình
            if (User.IsInRole("Doctor"))
            {
                var doctorUserId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

                var doctor = await _context.Doctors
                    .FirstOrDefaultAsync(d => d.UserId == doctorUserId);

                if (doctor == null)
                    return Forbid();

                var hasAppointment = await _context.Appointments
                    .AnyAsync(a => a.PatientId == id && a.DoctorId == doctor.Id);

                if (!hasAppointment)
                    return Forbid();
            }

            return Ok(patient);
        }

        // ============================================================
        // GET: /api/patients/my
        // Doctor: xem danh sách bệnh nhân có lịch với mình
        // ============================================================
        [HttpGet("my")]
        [Authorize(Roles = "Doctor")]
        public async Task<IActionResult> GetMyPatients()
        {
            var doctorUserId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var doctor = await _context.Doctors
                .FirstOrDefaultAsync(d => d.UserId == doctorUserId);

            if (doctor == null)
                return Forbid();

            var patients = await _context.Appointments
                .Where(a => a.DoctorId == doctor.Id)
                .Select(a => a.Patient)
                .Distinct()
                .AsNoTracking()
                .ToListAsync();

            return Ok(patients);
        }

        // ============================================================
        // PUT: /api/patients/{id}
        // Staff: cập nhật thông tin hành chính
        // ============================================================
        [HttpPut("{id:guid}")]
        [Authorize(Roles = "Staff")]
        public async Task<IActionResult> UpdatePatient(Guid id, [FromBody] UpdatePatientRequest request)
        {
            var patient = await _context.Patients.FindAsync(id);

            if (patient == null)
                return NotFound(new { message = "Patient not found." });

            // Chỉ cập nhật thông tin hành chính
            patient.Address = request.Address;
            patient.Note = request.Note;
            patient.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Patient updated successfully." });
        }
    }

    // ============================================================
    // DTO dùng cho PUT
    // ============================================================
    public class UpdatePatientRequest
    {
        public string? Address { get; set; }
        public string? Note { get; set; }
    }
}