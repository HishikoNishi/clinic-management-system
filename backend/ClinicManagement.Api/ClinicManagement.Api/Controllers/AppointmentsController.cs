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

namespace ClinicManagement.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AppointmentsController : ControllerBase
    {
        private readonly ClinicDbContext _context;
        private readonly OtpService _otpService;
        private readonly EmailService _emailService;

        public AppointmentsController(
            ClinicDbContext context,
            OtpService otpService,
            EmailService emailService)
        {
            _context = context;
            _otpService = otpService;
            _emailService = emailService;
        }

        // ================= CREATE =================
        [HttpPost]
        public async Task<IActionResult> Create(CreateAppointmentDto dto)
        {
            var today = DateTime.UtcNow.Date;
            var businessStart = new TimeSpan(7, 0, 0);
            var businessEnd = new TimeSpan(22, 0, 0);

            // ================= TRIM =================
            dto.CitizenId = dto.CitizenId?.Trim();
            dto.Phone = dto.Phone?.Trim();
            dto.FullName = dto.FullName?.Trim();
            dto.Email = dto.Email?.Trim();
            dto.InsuranceCardNumber = dto.InsuranceCardNumber?.Trim();

            // ================= VALIDATE =================
            if (dto.AppointmentDate.Date < today)
                return BadRequest("Chỉ được đặt lịch từ hôm nay trở đi");

            if (dto.AppointmentTime < businessStart || dto.AppointmentTime > businessEnd)
                return BadRequest("Chỉ nhận đặt lịch 07:00 - 22:00");

            if (string.IsNullOrWhiteSpace(dto.Email))
                return BadRequest("Email bắt buộc");

            if (!string.IsNullOrWhiteSpace(dto.CitizenId) &&
                (!dto.CitizenId.All(char.IsDigit) || dto.CitizenId.Length != 12))
            {
                return BadRequest("CCCD phải gồm 12 chữ số");
            }

            var verified = await _otpService.IsVerifiedAsync(dto.Email);
            if (!verified)
                return BadRequest("Email chưa xác thực OTP");

            // ================= FIND PATIENT =================
            Patient? patient = null;

            // ưu tiên CCCD
            if (!string.IsNullOrWhiteSpace(dto.CitizenId))
            {
                patient = await _context.Patients
                    .FirstOrDefaultAsync(p => p.CitizenId == dto.CitizenId);
            }

            // fallback theo tên + sđt
            if (patient == null)
            {
                patient = await _context.Patients
                    .FirstOrDefaultAsync(p =>
                        p.Phone == dto.Phone &&
                        p.FullName == dto.FullName);
            }

            // ================= CREATE / UPDATE =================
            if (patient == null)
            {
              

                // tạo mới
                patient = new Patient
                {
                    Id = Guid.NewGuid(),
                    FullName = dto.FullName,
                    Phone = dto.Phone,
                    Email = dto.Email,
                    Address = dto.Address,
                    DateOfBirth = dto.DateOfBirth,
                    Gender = dto.Gender,
                    CitizenId = dto.CitizenId,
                    InsuranceCardNumber = dto.InsuranceCardNumber
                };

                _context.Patients.Add(patient);
                await _context.SaveChangesAsync();
            }
            else
            {
                // update CCCD (nếu chưa có)
                if (string.IsNullOrWhiteSpace(patient.CitizenId) &&
                    !string.IsNullOrWhiteSpace(dto.CitizenId))
                {
                    patient.CitizenId = dto.CitizenId;
                }

                // update BHYT
                if (string.IsNullOrWhiteSpace(patient.InsuranceCardNumber) &&
                    !string.IsNullOrWhiteSpace(dto.InsuranceCardNumber))
                {
                    patient.InsuranceCardNumber = dto.InsuranceCardNumber;
                }

                await _context.SaveChangesAsync();
            }

            // ================= CHECK TRÙNG LỊCH =================
            var existed = await _context.Appointments.AnyAsync(a =>
                a.PatientId == patient.Id &&
                a.AppointmentDate == dto.AppointmentDate.Date &&
                a.AppointmentTime == dto.AppointmentTime);

            if (existed)
                return BadRequest("Bạn đã đặt lịch giờ này rồi");

            // ================= CREATE APPOINTMENT =================
            string code;
            do
            {
                code = CodeGenerator.GenerateAppointmentCode();
            }
            while (await _context.Appointments.AnyAsync(a => a.AppointmentCode == code));

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

            // ================= RESPONSE =================
            return Ok(new AppointmentDetailDto
            {
                Id = appointment.Id,
                AppointmentCode = appointment.AppointmentCode,
                FullName = patient.FullName,
                Phone = patient.Phone,
                Email = patient.Email,
                DateOfBirth = patient.DateOfBirth,
                Gender = patient.Gender.ToString(),
                Address = patient.Address,
                Reason = appointment.Reason,
                Status = appointment.Status.ToString(),
                AppointmentDate = appointment.AppointmentDate,
                AppointmentTime = appointment.AppointmentTime,
                CreatedAt = appointment.CreatedAt,
                PatientCode = patient.PatientCode,
                CitizenId = patient.CitizenId,
                InsuranceCardNumber = patient.InsuranceCardNumber
            });
        }

        // ================= LOOKUP PATIENT =================
        [HttpGet("patient-lookup")]
        [AllowAnonymous]
        public async Task<IActionResult> LookupPatient(
            [FromQuery] string? phone,
            [FromQuery] string? email)
        {
            if (string.IsNullOrWhiteSpace(phone) && string.IsNullOrWhiteSpace(email))
                return BadRequest("Nhập SĐT hoặc email");

            var patient = await _context.Patients
                .AsNoTracking()
                .FirstOrDefaultAsync(p =>
                    (!string.IsNullOrWhiteSpace(phone) && p.Phone == phone) ||
                    (!string.IsNullOrWhiteSpace(email) && p.Email == email));

            if (patient == null)
                return NotFound("Không tìm thấy");

            return Ok(new
            {
                patient.FullName,
                patient.DateOfBirth,
                patient.Gender,
                patient.Phone,
                patient.Email,
                patient.Address,
                patient.PatientCode,
                patient.CitizenId,
                patient.InsuranceCardNumber
            });
        }

        // ================= GET BY CODE =================
        [HttpGet("{code}")]
        public async Task<IActionResult> GetByCode(string code)
        {
            var appointment = await _context.Appointments
                .Include(a => a.Patient)
                .FirstOrDefaultAsync(a => a.AppointmentCode == code);

            if (appointment == null)
                return NotFound();

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
                CreatedAt = appointment.CreatedAt,
                PatientCode = appointment.Patient.PatientCode,
                CitizenId = appointment.Patient.CitizenId,
                InsuranceCardNumber = appointment.Patient.InsuranceCardNumber
            });
        }
    }
}