using System;
using System.Security.Claims;
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
    [Route("api/doctor/[controller]")]
    [Authorize(Roles = "Doctor")]
    public class DoctorAppointmentsController : ControllerBase
    {
        private readonly ClinicDbContext _context;

        public DoctorAppointmentsController(ClinicDbContext context)
        {
            _context = context;
        }

        // GET: api/doctor/Appointments
        [HttpGet]
        public async Task<ActionResult<IEnumerable<AppointmentDetailDto>>> GetAppointmentsForDoctor()
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
            var appointments = await _context.Appointments
                .Include(a => a.Patient)
                .Where(a => a.DoctorId == doctor.Id)
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
