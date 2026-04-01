using System;
using System.Security.Claims;
using ClinicManagement.Api.Data;
using ClinicManagement.Api.Dtos.Appointments;
using ClinicManagement.Api.DTOs.Appointments;
using ClinicManagement.Api.Dtos.MedicalRecords;
using System.Linq;
using ClinicManagement.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Api.Controllers
{
    [ApiController]
    [Route("api/doctor/[controller]")]
    [Authorize(Roles = "Doctor")]
    public class DoctorAppointmentsController : ControllerBase
    {
        private readonly ClinicDbContext _context;

        public DoctorAppointmentsController(ClinicDbContext context)
        {
            _context = context;
        }
        [HttpGet("{id}")]
        public async Task<ActionResult<AppointmentDetailDto>> GetAppointmentDetail(Guid id)
        {
            // L?y userId t? token
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdClaim, out Guid userId))
                return Unauthorized("Invalid user id in token");

            // Tìm doctor theo userId
            var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.UserId == userId);
            if (doctor == null) return Unauthorized("Doctor not found");

            // L?y appointment thu?c v? doctor này
            var appointment = await _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Doctor).ThenInclude(d => d.User)
                .FirstOrDefaultAsync(a => a.Id == id && a.DoctorId == doctor.Id);

            if (appointment == null) return NotFound();

            var dto = new AppointmentDetailDto
            {
                Id = appointment.Id,
                AppointmentCode = appointment.AppointmentCode,
                FullName = appointment.Patient?.FullName ?? string.Empty,
                Phone = appointment.Patient?.Phone ?? string.Empty,
                Email = appointment.Patient?.Email,
                DateOfBirth = appointment.Patient?.DateOfBirth ?? DateTime.MinValue,
                Gender = appointment.Patient?.Gender.ToString() ?? string.Empty,
                Address = appointment.Patient?.Address,
                Reason = appointment.Reason,
                Status = appointment.Status.ToString(),
                AppointmentDate = appointment.AppointmentDate,
                AppointmentTime = appointment.AppointmentTime,
                CreatedAt = appointment.CreatedAt,
                StatusDetail = new AppointmentStatusDto
                {
                    Value = appointment.Status.ToString(),
                    DoctorName = (appointment.Status == AppointmentStatus.Confirmed || appointment.Status == AppointmentStatus.Completed)
                                 ? appointment.Doctor?.User?.Username
                                 : null,
                    DoctorCode = (appointment.Status == AppointmentStatus.Confirmed || appointment.Status == AppointmentStatus.Completed)
                                 ? appointment.Doctor?.Code
                                 : null
                }
            };

            return Ok(dto);
        }


        // GET: api/doctor/Appointments
        [HttpGet]
        public async Task<ActionResult<IEnumerable<AppointmentDetailDto>>> GetAppointmentsForDoctor([FromQuery] string? status = null)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!Guid.TryParse(userIdClaim, out Guid userId))
                return Unauthorized("Invalid user id in token");
      
            var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.UserId == userId);
            if (doctor == null) return Unauthorized("Doctor not found");
            foreach (var claim in User.Claims)
            {
                Console.WriteLine($"{claim.Type} : {claim.Value}");
            }
            var appointmentsQuery = _context.Appointments
                .Include(a => a.Patient)
                .Where(a => a.DoctorId == doctor.Id);

            if (!string.IsNullOrWhiteSpace(status))
            {
                var statusList = status.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                       .Select(s => s.Trim())
                                       .Select(s => Enum.TryParse<AppointmentStatus>(s, true, out var parsed) ? parsed : (AppointmentStatus?)null)
                                       .Where(s => s.HasValue)
                                       .Select(s => s!.Value)
                                       .ToList();

                if (statusList.Any())
                {
                    appointmentsQuery = appointmentsQuery.Where(a => statusList.Contains(a.Status));
                }
            }

            var appointments = await appointmentsQuery
               .Select(a => new AppointmentDetailDto
               {
                   Id = a.Id,
                   AppointmentCode = a.AppointmentCode,
                   FullName = a.Patient != null ? a.Patient.FullName : null,
                   Phone = a.Patient != null ? a.Patient.Phone : null,
                   Email = a.Patient != null ? a.Patient.Email : null,
                   DateOfBirth = a.Patient != null ? a.Patient.DateOfBirth : DateTime.MinValue,
                   Gender = a.Patient != null ? a.Patient.Gender.ToString() : null,
                   Address = a.Patient != null ? a.Patient.Address : null,
                   Reason = a.Reason,
                   Status = a.Status.ToString(),
                   AppointmentDate = a.AppointmentDate,
                   AppointmentTime = a.AppointmentTime,
                   CreatedAt = a.CreatedAt,
                   StatusDetail = new AppointmentStatusDto
                   {
                       Value = a.Status.ToString(),
                       DoctorName = (a.Status == AppointmentStatus.Confirmed || a.Status == AppointmentStatus.Completed) && a.Doctor != null
                                     ? a.Doctor.User.Username   // l?y username t? User
                                     : null,
                       DoctorCode = (a.Status == AppointmentStatus.Confirmed || a.Status == AppointmentStatus.Completed) && a.Doctor != null
                                     ? a.Doctor.Code
                                     : null
                   }

               })
                .ToListAsync();

            return Ok(appointments);
        }

        [HttpGet("{id}/examination")]
        public async Task<ActionResult<ExaminationDetailDto>> GetExaminationDetail(Guid id)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!Guid.TryParse(userIdClaim, out Guid userId))
                return Unauthorized("Invalid user id in token");

            var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.UserId == userId);
            if (doctor == null) return Unauthorized("Doctor not found");

            var appointment = await _context.Appointments
                .Include(a => a.Patient)
                .FirstOrDefaultAsync(a => a.Id == id && a.DoctorId == doctor.Id);

            if (appointment == null) return NotFound();

            var history = await _context.MedicalRecords
                .Where(r => r.PatientId == appointment.PatientId)
                .OrderByDescending(r => r.CreatedAt)
                .Take(5)
                .Select(r => new BasicMedicalHistoryDto
                {
                    Id = r.Id,
                    CreatedAt = r.CreatedAt,
                    Diagnosis = r.Diagnosis,
                    Note = r.Note
                })
                .ToListAsync();

            var dto = new ExaminationDetailDto
            {
                AppointmentId = appointment.Id,
                Appointment = new AppointmentDetailDto
                {
                    Id = appointment.Id,
                    AppointmentCode = appointment.AppointmentCode,
                    FullName = appointment.Patient?.FullName,
                    Phone = appointment.Patient?.Phone,
                    Email = appointment.Patient?.Email,
                    DateOfBirth = appointment.Patient?.DateOfBirth ?? DateTime.MinValue,
                    Gender = appointment.Patient?.Gender.ToString(),
                    Address = appointment.Patient?.Address,
                    Reason = appointment.Reason,
                    Status = appointment.Status.ToString(),
                    AppointmentDate = appointment.AppointmentDate,
                    AppointmentTime = appointment.AppointmentTime,
                    CreatedAt = appointment.CreatedAt
                },
                MedicalHistory = history
            };

            return Ok(dto);
        }


        // PATCH: api/doctor/Appointments/{id}/complete
        [HttpPatch("{id}/complete")]
        public async Task<IActionResult> CompleteAppointment(Guid id)
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var doctor = await _context.Doctors
                .FirstOrDefaultAsync(d => d.UserId == userId);

            var appointment = await _context.Appointments
                .FirstOrDefaultAsync(a => a.Id == id && a.DoctorId == doctor.Id);
            if (appointment == null)
                return NotFound("Appointment not found or not assigned to this doctor");

            appointment.Status = AppointmentStatus.Completed;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Appointment marked as completed" });
        }
    }
}
