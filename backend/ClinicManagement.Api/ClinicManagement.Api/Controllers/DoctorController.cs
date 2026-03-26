using ClinicManagement.Api.Data;
using ClinicManagement.Api.Dtos.Doctor;
using ClinicManagement.Api.Dtos.Patients;
using ClinicManagement.Api.DTOs.Appointments;
using ClinicManagement.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    
    public class DoctorController : ControllerBase
    {
        private readonly ClinicDbContext _context;

        public DoctorController(ClinicDbContext context)
        {
            _context = context;
        }

        /* ================= GET ALL ================= */
        [HttpGet]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> GetAll()
        {
            var doctors = await _context.Doctors
                .Include(d => d.User)
                  .Include(d => d.Department)
                .Select(d => new DoctorDto
                {
                    Id = d.Id,
                    Code = d.Code,
                    Specialty = d.Specialty,
                    LicenseNumber = d.LicenseNumber,
                    FullName = d.FullName,
                    Username = d.User.Username,
                    Status = d.Status.ToString(),
                    DepartmentId = (Guid)d.DepartmentId,
                    DepartmentName = d.Department.Name
                })
                .ToListAsync();

            return Ok(doctors);
        }
        [Authorize(Roles = "Admin,Staff")]
        /* ================= GET BY ID ================= */
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> Get(Guid id)
        {
            var doctor = await _context.Doctors
                .Include(d => d.User)
                  .Include(d => d.Department)
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
                Status = doctor.Status.ToString(),
                DepartmentId = doctor.DepartmentId,
                DepartmentName = doctor.Department.Name
            });
        }
        [Authorize(Roles = "Admin")]
        /* ================= CREATE ================= */
        [HttpPost]
        public async Task<IActionResult> Create(CreateDoctorDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Kiểm tra User có tồn tại không
            var user = await _context.Users.FindAsync(dto.UserId);
            if (user == null)
                return BadRequest(new { message = "User not found." });

            // Kiểm tra User đã có Doctor profile chưa
            if (await _context.Doctors.AnyAsync(d => d.UserId == dto.UserId))
                return BadRequest(new { message = "This user already has a doctor profile." });

            // Kiểm tra Code có trùng không
            if (await _context.Doctors.AnyAsync(d => d.Code == dto.Code))
                return BadRequest(new { message = "Doctor code already exists." });

            var doctor = new Doctor
            {
                Id = Guid.NewGuid(),
                Code = dto.Code,
                FullName = dto.FullName,
                Specialty = dto.Specialty,
                LicenseNumber = dto.LicenseNumber ?? string.Empty,
                Status = DoctorStatus.Active,
                UserId = dto.UserId,
                DepartmentId = dto.DepartmentId,
                CreatedAt = DateTime.UtcNow
            };

            await _context.Doctors.AddAsync(doctor);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(Get), new { id = doctor.Id }, new
            {
                message = "Doctor profile created successfully",
                doctorId = doctor.Id
            });
        }
        [Authorize(Roles = "Admin")]
        /* ================= UPDATE ================= */
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, UpdateDoctorDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var doctor = await _context.Doctors.FindAsync(id);
            if (doctor == null)
                return NotFound();

            if (doctor.Code != dto.Code &&
                await _context.Doctors.AnyAsync(d => d.Code == dto.Code))
                return BadRequest(new { message = "Doctor code already exists." });

            doctor.Code = dto.Code;
            doctor.FullName = dto.FullName;
            doctor.Specialty = dto.Specialty;
            doctor.LicenseNumber = dto.LicenseNumber ?? string.Empty;
            doctor.DepartmentId = dto.DepartmentId;


            await _context.SaveChangesAsync();

            return Ok(new { message = "Doctor profile updated successfully." });
        }
        [Authorize(Roles = "Admin")]
        /* ================= DELETE ================= */
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var doctor = await _context.Doctors.FindAsync(id);
            if (doctor == null)
                return NotFound();

            _context.Doctors.Remove(doctor);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Doctor profile deleted successfully." });
        }
        [HttpGet("by-department/{departmentId:guid}")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> GetByDepartment(Guid departmentId)
        {
            var doctors = await _context.Doctors
                .Where(d => d.DepartmentId == departmentId)
                .Select(d => new
                {
                    d.Id,
                    d.FullName,
                    d.Specialty
                })
                .ToListAsync();

            return Ok(doctors);
        }
        [HttpGet("{id}/appointments")]
        [Authorize(Roles = "Staff")]
        public async Task<ActionResult<IEnumerable<AppointmentDetailDto>>> GetAppointmentsByDoctor(Guid id)
        {
            var appointments = await _context.Appointments
                .Include(a => a.Patient)
                .Where(a => a.DoctorId == id)
                .Select(a => new AppointmentDetailDto
                {
                    Id = a.Id,
                    AppointmentCode = a.AppointmentCode,
                    Reason = a.Reason,
                    Status = a.Status.ToString(),
                    AppointmentDate = a.AppointmentDate,
                    AppointmentTime = a.AppointmentTime,
                    CreatedAt = a.CreatedAt,
                    FullName = a.Patient.FullName,
                    Phone = a.Patient.Phone,
                    Email = a.Patient.Email
                })
                .ToListAsync();

            return Ok(appointments);
        }


    }
}