using ClinicManagement.Api.Data;
using ClinicManagement.Api.DTOs.Appointments;
using ClinicManagement.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ClinicManagement.Api.Utils;
using ClinicManagement.Api.Dtos.Appointments;
using ClinicManagement.Api.DTOs;

namespace ClinicManagement.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AppointmentsController : ControllerBase
    {
        private readonly ClinicDbContext _context;

        public AppointmentsController(ClinicDbContext context)
        {
            _context = context;
        }
        [HttpPost]
        public async Task<IActionResult> Create( CreateAppointmentDto dto)
        {
            // 1️⃣ tìm patient theo SĐT
            var patient = await _context.Patients
                .FirstOrDefaultAsync(p => p.Phone == dto.Phone);

            // 2️⃣ chưa có → tạo mới
            if (patient == null)
            {
                patient = new Patient
                {
                    Id = Guid.NewGuid(),
                    FullName = dto.FullName,
                    Phone = dto.Phone,
                    Email = dto.Email,
                    DateOfBirth = dto.DateOfBirth,
                    Gender = dto.Gender
                };

                _context.Patients.Add(patient);
            }

            // 3️⃣ chặn đặt trùng giờ
            var existed = await _context.Appointments.AnyAsync(a =>
                a.PatientId == patient.Id &&
                a.AppointmentDate == dto.AppointmentDate.Date &&
                a.AppointmentTime == dto.AppointmentTime);

            if (existed)
                return BadRequest("Bạn đã đặt lịch giờ này rồi");

            // ✅ 4️⃣ TẠO MÃ KHÁM (ĐẶT Ở ĐÂY)
            string code;

            do
            {
                code = CodeGenerator.GenerateAppointmentCode();
            }
            while (await _context.Appointments
                .AnyAsync(a => a.AppointmentCode == code));

            // 5️⃣ tạo appointment
            var appointment = new Appointment
            {
                Id = Guid.NewGuid(),
                PatientId = patient.Id,
                AppointmentCode = code,
                AppointmentDate = dto.AppointmentDate.Date,
                AppointmentTime = dto.AppointmentTime,
                Reason = dto.Reason,
                Status = AppointmentStatus.Pending
            };

            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();

            // ✅ 6️⃣ trả mã cho guest xem
            return Ok(new
            {
                message = "Đặt lịch thành công",

                fullName = patient.FullName,
                phone = patient.Phone,

                Status = appointment.Status.ToString(),
                appointmentCode = appointment.AppointmentCode,
                appointmentDate = appointment.AppointmentDate,
                appointmentTime = appointment.AppointmentTime
            });

        }
        [HttpGet("{code}")]
        public async Task<IActionResult> GetByCode(string code)
        {
            var appointment = await _context.Appointments
                .Include(a => a.Patient)
                .FirstOrDefaultAsync(a => a.AppointmentCode == code);

            if (appointment == null) return NotFound("Không tìm thấy lịch khám");

            return Ok(new AppointmentDetailDto
            {
                FullName = appointment.Patient.FullName,
                Phone = appointment.Patient.Phone,
                AppointmentCode = appointment.AppointmentCode,
                AppointmentDate = appointment.AppointmentDate,
                AppointmentTime = appointment.AppointmentTime,
                Reason = appointment.Reason,
                Status = appointment.Status.ToString(),
                Address = appointment.Patient.Address
                      });
            }

        [HttpPost("search")]
        public async Task<IActionResult> Search(SearchAppointmentDto dto)
        {
            var appointment = await _context.Appointments
             .Include(x => x.Patient)
             .FirstOrDefaultAsync(x =>
                 x.AppointmentCode == dto.AppointmentCode &&
                 x.Patient.Phone == dto.Phone);

            if (appointment == null)
                return NotFound();

            return Ok(appointment);
        }

        [HttpPost("cancel")]
        public async Task<IActionResult> Cancel(CancelAppointmentDto dto)
        {
            var appointment = await _context.Appointments
                .Include(a => a.Patient)
                .FirstOrDefaultAsync(a =>
                    a.AppointmentCode == dto.AppointmentCode &&
                    a.Patient.FullName == dto.FullName &&
                    a.Patient.Phone == dto.Phone);

            if (appointment == null)
                return NotFound("Không tìm thấy lịch khám");

            appointment.Status = AppointmentStatus.Cancelled;

            await _context.SaveChangesAsync();

            return Ok("Huỷ lịch thành công");
        }



    }
        

}
