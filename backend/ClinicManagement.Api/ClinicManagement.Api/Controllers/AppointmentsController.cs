using ClinicManagement.Api.Data;
using ClinicManagement.Api.Dtos.Appointments;
using ClinicManagement.Api.DTOs;
using ClinicManagement.Api.DTOs.Appointments;
using ClinicManagement.Api.Models;
using ClinicManagement.Api.Services;
using ClinicManagement.Api.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data;
namespace ClinicManagement.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AppointmentsController : ControllerBase
    {
        private readonly ClinicDbContext _context;
        private readonly OtpService _otpService;
        private readonly EmailService _emailService;

        public AppointmentsController(ClinicDbContext context, OtpService otpService, EmailService emailService)
        {
            _context = context;
            _otpService = otpService;
            _emailService = emailService;
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateAppointmentDto dto)
        {
            var today = DateTime.UtcNow.Date;
            var businessStart = new TimeSpan(7, 0, 0);
            var businessEnd = new TimeSpan(22, 0, 0);

            if (dto.AppointmentDate.Date < today)
                return BadRequest(new { message = "Chỉ được đặt lịch từ hôm nay trở đi" });

            if (dto.AppointmentTime < businessStart || dto.AppointmentTime > businessEnd)
                return BadRequest(new { message = "Chỉ nhận đặt lịch trong khung giờ làm việc (07:00-22:00)" });

            if (string.IsNullOrWhiteSpace(dto.Email))
                return BadRequest(new { message = "Email là bắt buộc để xác thực OTP" });

            var verified = await _otpService.IsVerifiedAsync(dto.Email);
            if (!verified)
                return BadRequest(new { message = "Email chưa được xác thực OTP hoặc OTP đã hết hạn" });

            // 1️⃣ tìm patient theo SĐT + Tên (tránh trùng người)
            var patient = await _context.Patients
                .FirstOrDefaultAsync(p =>
                    p.Phone == dto.Phone &&
                    p.FullName == dto.FullName);

            // 2️⃣ chưa có → tạo mới
            if (patient == null)
            {
                patient = new Patient
                {
                    Id = Guid.NewGuid(),
                    FullName = dto.FullName,
                    Phone = dto.Phone,
                    Email = dto.Email,
                    Address = dto.Address,
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
            var responseDto = new AppointmentDetailDto
            {
                Id = appointment.Id,
                AppointmentCode = appointment.AppointmentCode,
                FullName = appointment.Patient.FullName,
                Phone = appointment.Patient.Phone,
                Email = appointment.Patient.Email,
                DateOfBirth = appointment.Patient.DateOfBirth,
                Gender = appointment.Patient.Gender.ToString(),
                Address = appointment.Patient.Address,
                Reason = appointment.Reason,
                Status = appointment.Status.ToString(),
                AppointmentDate = appointment.AppointmentDate,
                AppointmentTime = appointment.AppointmentTime,
                CreatedAt = appointment.CreatedAt
            };

            try
            {
                if (!string.IsNullOrWhiteSpace(dto.Email))
                {
                    await _emailService.SendAsync(dto.Email, "Xác nhận lịch khám",
                        $"Mã khám: <b>{appointment.AppointmentCode}</b><br/>Họ tên: {appointment.Patient.FullName}<br/>Ngày: {appointment.AppointmentDate:dd/MM/yyyy} - Giờ: {appointment.AppointmentTime}");
                }
            }
            catch (Exception)
            {
                // log email failure quietly
            }

            return Ok(responseDto);

        }

        [HttpGet("patient-lookup")]
        [AllowAnonymous]
        public async Task<IActionResult> LookupPatient([FromQuery] string? phone, [FromQuery] string? email)
        {
            if (string.IsNullOrWhiteSpace(phone) && string.IsNullOrWhiteSpace(email))
                return BadRequest("Cần nhập SĐT hoặc email");

            var patient = await _context.Patients
                .AsNoTracking()
                .FirstOrDefaultAsync(p =>
                    (!string.IsNullOrWhiteSpace(phone) && p.Phone == phone) ||
                    (!string.IsNullOrWhiteSpace(email) && p.Email == email));

            if (patient == null)
                return NotFound(new { message = "Không tìm thấy bệnh nhân" });

            return Ok(new
            {
                patient.FullName,
                patient.DateOfBirth,
                patient.Gender,
                patient.Phone,
                patient.Email,
                patient.Address
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
                Id = appointment.Id,
                AppointmentCode = appointment.AppointmentCode,
                FullName = appointment.Patient.FullName,
                Phone = appointment.Patient.Phone,
                Email = appointment.Patient.Email,
                DateOfBirth = appointment.Patient.DateOfBirth,
                Gender = appointment.Patient.Gender.ToString(),
                Address = appointment.Patient.Address,
                Reason = appointment.Reason,
                Status = appointment.Status.ToString(),
                AppointmentDate = appointment.AppointmentDate,
                AppointmentTime = appointment.AppointmentTime,
                CreatedAt = appointment.CreatedAt
            });

        }

        [HttpPost("search")]
        public async Task<IActionResult> Search(SearchAppointmentDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Phone) && string.IsNullOrWhiteSpace(dto.Email))
                return BadRequest(new { message = "Cần nhập số điện thoại hoặc email" });

            var query = _context.Appointments
                .Include(x => x.Patient)
                .AsQueryable();

            query = query.Where(x =>
                (!string.IsNullOrWhiteSpace(dto.Phone) && x.Patient.Phone == dto.Phone) ||
                (!string.IsNullOrWhiteSpace(dto.Email) && x.Patient.Email == dto.Email));

            if (!string.IsNullOrWhiteSpace(dto.AppointmentCode))
            {
                query = query.Where(x => x.AppointmentCode == dto.AppointmentCode);
            }

            var appointments = await query
                .OrderByDescending(x => x.CreatedAt)
                .Take(50)
                .ToListAsync();

            var result = appointments.Select(appointment => new AppointmentDetailDto
            {
                Id = appointment.Id,
                AppointmentCode = appointment.AppointmentCode,
                FullName = appointment.Patient.FullName,
                Phone = appointment.Patient.Phone,
                Email = appointment.Patient.Email,
                DateOfBirth = appointment.Patient.DateOfBirth,
                Gender = appointment.Patient.Gender.ToString(),
                Address = appointment.Patient.Address,
                Reason = appointment.Reason,
                Status = appointment.Status.ToString(),
                AppointmentDate = appointment.AppointmentDate,
                AppointmentTime = appointment.AppointmentTime,
                CreatedAt = appointment.CreatedAt
            }).ToList();

            return Ok(result);

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
