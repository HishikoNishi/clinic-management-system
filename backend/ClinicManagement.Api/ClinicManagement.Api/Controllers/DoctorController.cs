using System;
using System.Threading.Tasks;
using ClinicManagement.Api.Data;
using ClinicManagement.Api.Dtos.Doctor;
using ClinicManagement.Api.Dtos.Patients;
using ClinicManagement.Api.DTOs.Appointments;
using ClinicManagement.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Extensions;

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
        [HttpGet("departments")]
        [Authorize(Roles = "Admin,Staff,Doctor")]
        public async Task<IActionResult> GetDepartments()
        {
            var departments = await _context.Departments
                .Select(d => new
                {
                    d.Id,
                    d.Name
                })
                .ToListAsync();

            return Ok(departments);
        }

        /* ================= GET ALL ================= */
        [HttpGet]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> GetAll()
        {
            var doctors = await _context.Doctors
    .Include(d => d.User)
    .Include(d => d.Department)
    .Include(d => d.Specialty)
    .Select(d => new DoctorDto
    {
        Id = d.Id,
        Code = d.Code,
        FullName = d.FullName,
        SpecialtyId = d.SpecialtyId,
        SpecialtyName = d.Specialty.Name,
        LicenseNumber = d.LicenseNumber,
        Username = d.User.Username,
        Status = d.Status.ToString(),
        DepartmentId = d.DepartmentId,
        DepartmentName = d.Department.Name,
        AvatarUrl = d.AvatarUrl
    })
    .ToListAsync();



            return Ok(doctors);
        }
        [Authorize(Roles = "Admin,Staff,Doctor")]        /* ================= GET BY ID ================= */
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
                FullName = doctor.FullName,
                SpecialtyId = doctor.SpecialtyId,
                SpecialtyName = doctor.Specialty.Name,
                LicenseNumber = doctor.LicenseNumber,
                Username = doctor.User.Username,
                Status = doctor.Status.ToString(),
                DepartmentId = doctor.DepartmentId,
                DepartmentName = doctor.Department.Name,
                AvatarUrl = doctor.AvatarUrl
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
                SpecialtyId = dto.SpecialtyId,
                LicenseNumber = dto.LicenseNumber ?? string.Empty,
                Status = DoctorStatus.Active,
                UserId = dto.UserId,
                AvatarUrl = dto.AvatarUrl,
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
        [HttpPut("{id:guid}/status")]
        [Authorize(Roles = "Admin,Doctor")]
        public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateDoctorStatusDto dto)
        {
            var doctor = await _context.Doctors.FindAsync(id);
            if (doctor == null) return NotFound();

            doctor.Status = dto.Status;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Doctor status updated successfully.", status = doctor.Status.ToString() });
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
            doctor.SpecialtyId = dto.SpecialtyId;
            doctor.LicenseNumber = dto.LicenseNumber ?? string.Empty;
            doctor.DepartmentId = dto.DepartmentId;
            doctor.AvatarUrl = dto.AvatarUrl;

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
        [HttpGet("departments/{departmentId}/specialties")]
        [Authorize(Roles = "Admin,Staff,Doctor")]
        public async Task<IActionResult> GetSpecialtiesByDepartment(Guid departmentId)
        {
            var specialties = await _context.Specialties
                .Where(s => s.DepartmentId == departmentId)
                .Select(s => new
                {
                    s.Id,
                    s.Name
                })
                .ToListAsync();

            if (!specialties.Any())
                return NotFound(new { message = "Không tìm thấy chuyên khoa cho khoa này." });

            return Ok(specialties);
        }


        [HttpGet("{id}/appointments")]
        [Authorize(Roles = "Admin,Staff")]
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
                    Email = a.Patient.Email,

                })
                .ToListAsync();

            return Ok(appointments);
        }


    }
}