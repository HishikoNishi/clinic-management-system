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

        [HttpPost]
        public async Task<IActionResult> Create(CreateAppointmentDto dto)
        {
            var today = DateTime.UtcNow.Date;
            var businessStart = new TimeSpan(7, 0, 0);
            var businessEnd = new TimeSpan(22, 0, 0);

            dto.CitizenId = dto.CitizenId?.Trim();
            dto.Phone = dto.Phone?.Trim();
            dto.FullName = dto.FullName?.Trim();
            dto.Email = dto.Email?.Trim();
            dto.InsuranceCardNumber = dto.InsuranceCardNumber?.Trim();

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

            Patient? patient = null;

            if (!string.IsNullOrWhiteSpace(dto.CitizenId))
            {
                patient = await _context.Patients
                    .FirstOrDefaultAsync(p => p.CitizenId == dto.CitizenId);
            }

            if (patient == null)
            {
                patient = await _context.Patients
                    .FirstOrDefaultAsync(p =>
                        p.Phone == dto.Phone &&
                        p.FullName == dto.FullName);
            }

            if (patient == null)
            {
                // Sinh mã bệnh nhân ngay lúc đăng ký (để Staff thấy được ở danh sách lịch).
                // Check-in phía Staff vẫn chỉ sinh nếu PatientCode đang thiếu.
                string patientCode;
                do
                {
                    patientCode = CodeGenerator.GeneratePatientCode();
                }
                while (await _context.Patients.AnyAsync(p => p.PatientCode == patientCode));

                patient = new Patient
                {
                    Id = Guid.NewGuid(),
                    PatientCode = patientCode,
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
                if (string.IsNullOrWhiteSpace(patient.PatientCode))
                {
                    string patientCode;
                    do
                    {
                        patientCode = CodeGenerator.GeneratePatientCode();
                    }
                    while (await _context.Patients.AnyAsync(p => p.PatientCode == patientCode));

                    patient.PatientCode = patientCode;
                }

                if (string.IsNullOrWhiteSpace(patient.CitizenId) &&
                    !string.IsNullOrWhiteSpace(dto.CitizenId))
                {
                    patient.CitizenId = dto.CitizenId;
                }

                if (string.IsNullOrWhiteSpace(patient.InsuranceCardNumber) &&
                    !string.IsNullOrWhiteSpace(dto.InsuranceCardNumber))
                {
                    patient.InsuranceCardNumber = dto.InsuranceCardNumber;
                }

                await _context.SaveChangesAsync();
            }

            var existed = await _context.Appointments.AnyAsync(a =>
                a.PatientId == patient.Id &&
                a.AppointmentDate == dto.AppointmentDate.Date &&
                a.AppointmentTime == dto.AppointmentTime);

            if (existed)
                return BadRequest("Bạn đã đặt lịch giờ này rồi");

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

            try
            {
                if (!string.IsNullOrWhiteSpace(dto.Email))
                {
                    await _emailService.SendAsync(
                        dto.Email,
                        "Xác nhận lịch khám",
                        $"Mã khám: <b>{appointment.AppointmentCode}</b><br/>Họ tên: {patient.FullName}<br/>Ngày: {appointment.AppointmentDate:dd/MM/yyyy} - Giờ: {appointment.AppointmentTime}");
                }
            }
            catch (Exception)
            {
                // ignore email failure
            }

            return Ok(ToDetailDto(appointment, patient));
        }

        [HttpGet("patient-lookup")]
        [AllowAnonymous]
        public async Task<IActionResult> LookupPatient([FromQuery] string? phone, [FromQuery] string? email)
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

        [HttpGet("{code}")]
        public async Task<IActionResult> GetByCode(string code)
        {
            var appointment = await _context.Appointments
                .Include(a => a.Patient)
                .FirstOrDefaultAsync(a => a.AppointmentCode == code);

            if (appointment == null || appointment.Patient == null)
                return NotFound();

            return Ok(ToDetailDto(appointment, appointment.Patient));
        }

        [HttpPost("search")]
        public async Task<IActionResult> Search(SearchAppointmentDto dto)
        {
            var hasIdentity =
                !string.IsNullOrWhiteSpace(dto.Phone) ||
                !string.IsNullOrWhiteSpace(dto.Email) ||
                !string.IsNullOrWhiteSpace(dto.CitizenId) ||
                !string.IsNullOrWhiteSpace(dto.InsuranceCardNumber);

            if (!hasIdentity)
                return BadRequest(new { message = "Cần nhập SĐT, email, CCCD hoặc BHYT" });

            var query = _context.Appointments
                .Include(x => x.Patient)
                .AsQueryable();

            query = query.Where(x =>
                (!string.IsNullOrWhiteSpace(dto.Phone) && x.Patient.Phone == dto.Phone) ||
                (!string.IsNullOrWhiteSpace(dto.Email) && x.Patient.Email == dto.Email) ||
                (!string.IsNullOrWhiteSpace(dto.CitizenId) && x.Patient.CitizenId == dto.CitizenId) ||
                (!string.IsNullOrWhiteSpace(dto.InsuranceCardNumber) && x.Patient.InsuranceCardNumber == dto.InsuranceCardNumber));

            if (!string.IsNullOrWhiteSpace(dto.AppointmentCode))
            {
                query = query.Where(x => x.AppointmentCode == dto.AppointmentCode);
            }

            var appointments = await query
                .OrderByDescending(x => x.CreatedAt)
                .Take(50)
                .ToListAsync();

            var result = appointments
                .Where(a => a.Patient != null)
                .Select(a => ToDetailDto(a, a.Patient!))
                .ToList();

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

        [HttpGet("no-show")]
        public async Task<IActionResult> GetNoShowAppointments()
        {
            var data = await _context.Appointments
                .Include(a => a.Patient)
                .Where(a => a.Status == AppointmentStatus.NoShow)
                .OrderByDescending(a => a.AppointmentDate)
                .ToListAsync();

            var result = data
                .Where(a => a.Patient != null)
                .Select(a => ToDetailDto(a, a.Patient!));

            return Ok(result);
        }

        [HttpGet("by-status")]
        public async Task<IActionResult> GetByStatus([FromQuery] AppointmentStatus status)
        {
            var data = await _context.Appointments
                .Include(a => a.Patient)
                .Where(a => a.Status == status)
                .OrderByDescending(a => a.AppointmentDate)
                .ToListAsync();

            var result = data
                .Where(a => a.Patient != null)
                .Select(a => ToDetailDto(a, a.Patient!));

            return Ok(result);
        }

        private static AppointmentDetailDto ToDetailDto(Appointment appointment, Patient patient)
        {
            return new AppointmentDetailDto
            {
                Id = appointment.Id,
                AppointmentCode = appointment.AppointmentCode,
                FullName = patient.FullName,
                Phone = patient.Phone ?? string.Empty,
                Email = patient.Email,
                DateOfBirth = patient.DateOfBirth,
                Gender = patient.Gender.ToString(),
                Address = patient.Address,
                Reason = appointment.Reason,
                Status = appointment.Status.ToString(),
                AppointmentDate = appointment.AppointmentDate,
                AppointmentTime = appointment.AppointmentTime,
                CreatedAt = appointment.CreatedAt,
                Note = patient.Note ?? string.Empty,
                PatientCode = patient.PatientCode,
                CitizenId = patient.CitizenId,
                InsuranceCardNumber = patient.InsuranceCardNumber
            };
        }
    }
}
