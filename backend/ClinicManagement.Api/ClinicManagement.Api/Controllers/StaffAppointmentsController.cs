using System;
using ClinicManagement.Api.Data;
using ClinicManagement.Api.Dtos.Appointments;
using ClinicManagement.Api.DTOs.Appointments;
using ClinicManagement.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Api.Controllers
{
    [ApiController]
    [Route("api/staff/[controller]")]
    [Authorize(Roles = "Staff")]
    public class StaffAppointmentsController : ControllerBase
    {
        private readonly ClinicDbContext _context;

        public StaffAppointmentsController(ClinicDbContext context)
        {
            _context = context;
        }

        // GET: api/staff/Appointments
        [HttpGet]
        public async Task<ActionResult<IEnumerable<AppointmentDetailDto>>> GetAppointments()
        {
            var appointments = await _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Doctor)
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
  

                })
                .ToListAsync();

            return Ok(appointments);
        }


        [HttpGet("filter")]
        public async Task<ActionResult<IEnumerable<AppointmentDetailDto>>> GetAppointmentsByStatus([FromQuery] AppointmentStatus status)
        {
            var appointments = await _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Doctor)
                    .ThenInclude(d => d.User) // thêm include User
                .Where(a => a.Status == status)
                .Select(a => new AppointmentDetailDto
                {
                    AppointmentCode = a.AppointmentCode,
                    FullName = a.Patient != null ? a.Patient.FullName : null,
                    Phone = a.Patient != null ? a.Patient.Phone : null,
                    Email = a.Patient != null ? a.Patient.Email : null,
                    DateOfBirth = a.Patient != null ? a.Patient.DateOfBirth : DateTime.MinValue,
                    Gender = a.Patient != null ? a.Patient.Gender.ToString() : null,
                    Address = a.Patient != null ? a.Patient.Address : null,
                    Reason = a.Reason,
                    AppointmentDate = a.AppointmentDate,
                    AppointmentTime = a.AppointmentTime,
                    CreatedAt = a.CreatedAt,

                    StatusDetail = new AppointmentStatusDto
                    {
                        Value = a.Status.ToString(),
                        DoctorName = (a.Status == AppointmentStatus.Confirmed || a.Status == AppointmentStatus.Completed) && a.Doctor != null
                                     ? a.Doctor.User.Username   // lấy username từ User
                                     : null,
                        DoctorCode = (a.Status == AppointmentStatus.Confirmed || a.Status == AppointmentStatus.Completed) && a.Doctor != null
                                     ? a.Doctor.Code
                                     : null
                    }
                })
                .ToListAsync();

            return Ok(appointments);
        }


        // POST: api/staff/Appointments/assign-doctor
        [HttpPost("assign-doctor")]
        public async Task<IActionResult> AssignDoctor([FromBody] AssignDoctorDto dto)
        {
            var appointment = await _context.Appointments
                .FirstOrDefaultAsync(a => a.Id == dto.AppointmentId);

            if (appointment == null)
                return NotFound("Appointment not found");

            var doctor = await _context.Doctors
                .FirstOrDefaultAsync(d => d.Id == dto.DoctorId);

            if (doctor == null)
                return NotFound("Doctor not found");

            appointment.DoctorId = doctor.Id;
            appointment.Status = AppointmentStatus.Confirmed;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Assigned doctor successfully",
                doctorCode = doctor.Code
            });
        }
    }
}
