using ClinicManagement.Api.Data;
using ClinicManagement.Api.Dtos.Invoices;
using ClinicManagement.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Api.Controllers
{
    [Authorize(Roles = "Staff")]
    [ApiController]
    [Route("api/appointments")]
    public class AppointmentManagementController : ControllerBase
    {
        private readonly ClinicDbContext _context;

        public AppointmentManagementController(ClinicDbContext context)
        {
            _context = context;
        }

        // ==============================
        // 1?? Xác nh?n l?ch h?n
        // ==============================
        [HttpPut("{id}/confirm")]
        public async Task<IActionResult> ConfirmAppointment(Guid id)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null)
                return NotFound(new { message = "Không tìm th?y l?ch h?n" });

            if (appointment.Status != AppointmentStatus.Pending)
                return BadRequest(new { message = "Ch? xác nh?n l?ch ?ang ? tr?ng thái Pending" });

            appointment.Status = AppointmentStatus.Confirmed;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Xác nh?n l?ch h?n thành công" });
        }

        // ==============================
        // 2?? Phân bác s?
        // ==============================
        [HttpPut("{id}/assign-doctor/{doctorId}")]
        public async Task<IActionResult> AssignDoctor(Guid id, Guid doctorId)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null)
                return NotFound(new { message = "Không tìm th?y l?ch h?n" });

            var doctor = await _context.Doctors.FindAsync(doctorId);
            if (doctor == null)
                return NotFound(new { message = "Không tìm th?y bác s?" });

            appointment.DoctorId = doctorId;
            appointment.Status = AppointmentStatus.Assigned;

            await _context.SaveChangesAsync();
            return Ok(new { message = "Phân bác s? thành công" });
        }

        // ==============================
        // 3?? Check-in b?nh nhân
        // ==============================
        [HttpPut("{id}/check-in")]
        public async Task<IActionResult> CheckIn(Guid id)
        {
            var appointment = await _context.Appointments.FindAsync(id);

            if (appointment == null)
                return NotFound(new { message = "Không tìm th?y l?ch h?n" });

            if (appointment.Status != AppointmentStatus.Assigned)
                return BadRequest(new { message = "L?ch ph?i ???c phân bác s? tr??c" });

            appointment.Status = AppointmentStatus.CheckedIn;
            await _context.SaveChangesAsync();

            return Ok(new { message = "B?nh nhân ?ã check-in" });
        }

        // ==============================
        // 6?? In phi?u khám
        // ==============================
        [HttpGet("{id}/print")]
        public async Task<IActionResult> PrintTicket(Guid id)
        {
            var appointment = await _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Doctor)
                .ThenInclude(d => d.User)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (appointment == null)
                return NotFound(new { message = "Không tìm th?y l?ch h?n" });

            return Ok(new
            {
                appointment.AppointmentCode,
                appointment.AppointmentDate,
                appointment.AppointmentTime,
                Patient = appointment.Patient?.FullName,
                Doctor = appointment.Doctor?.User?.Username,
                appointment.Reason
            });
        }
    }
}
