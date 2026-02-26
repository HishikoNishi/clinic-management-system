using ClinicManagement.Api.Data;
using ClinicManagement.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Api.Controllers
{
    [ApiController]
    [Route("api/staff/[controller]")]
    [Authorize(Roles = "Staff")]
    public class StaffDoctorsController : ControllerBase
    {
        private readonly ClinicDbContext _context;

        public StaffDoctorsController(ClinicDbContext context)
        {
            _context = context;
        }

        // STAFF tìm danh sách bác sĩ
        [HttpGet]
        public async Task<IActionResult> GetDoctors()
        {
            var doctors = await _context.Doctors
                .Include(d => d.User)
                .Select(d => new
                {
                    d.Id,
                    name = d.User.Username,
                    d.Code,
                    d.Specialty,
                    d.Status
                })
                .ToListAsync();

            return Ok(doctors);
        }

        //  STAFF xem bệnh nhân của bác sĩ
        [HttpGet("{id}/patients")]
        public async Task<IActionResult> GetDoctorPatients(Guid id)
        {
            var doctor = await _context.Doctors
                .Include(d => d.User)
                .Include(d => d.Appointments)
                    .ThenInclude(a => a.Patient)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (doctor == null)
                return NotFound("Doctor not found");

            var result = new
            {
                doctorName = doctor.User.Username,
                doctor.Specialty,
                patients = doctor.Appointments
                    .Where(a =>
                        a.Status == AppointmentStatus.Confirmed ||
                        a.Status == AppointmentStatus.Completed)
                    .Select(a => new
                    {
                        appointmentId = a.Id,
                        a.AppointmentCode,
                        patientName = a.Patient.FullName,
                        phone = a.Patient.Phone,
                        reason = a.Reason,
                        date = a.AppointmentDate,
                        time = a.AppointmentTime,
                        status = a.Status
                    })
            };

            return Ok(result);
        }
        /*
        // STAFF xem danh sách lịch hẹn chưa gán bác sĩ
        [HttpGet("unassigned")]
        public async Task<IActionResult> GetUnassigned()
        {
            var appointments = await _context.Appointments
                .Include(a => a.Patient)
                .Where(a => a.DoctorId == null)
                .Select(a => new
                {
                    a.Id,
                    a.AppointmentCode,
                    patient = a.Patient.FullName,
                    a.AppointmentDate,
                    a.AppointmentTime,
                    a.Status
                })
                .ToListAsync();

            return Ok(appointments);
        }
        */

    }
}
