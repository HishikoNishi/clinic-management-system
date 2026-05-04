using ClinicManagement.Api.Data;
using ClinicManagement.Api.Dtos.Appointments;
using ClinicManagement.Api.DTOs;
using ClinicManagement.Api.DTOs.Appointments;
using ClinicManagement.Api.Extensions;
using ClinicManagement.Api.Models;
using ClinicManagement.Api.Services;
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
        private readonly AppointmentBookingService _appointmentBookingService;

        public AppointmentsController(
            ClinicDbContext context,
            AppointmentBookingService appointmentBookingService)
        {
            _context = context;
            _appointmentBookingService = appointmentBookingService;
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateAppointmentDto dto)
        {
            // Use local date consistently (avoid allowing "yesterday" bookings around midnight due to UTC).
            var today = DateTime.Today;
            var businessStart = new TimeSpan(7, 0, 0);
            var businessEnd = new TimeSpan(22, 0, 0);

            _appointmentBookingService.NormalizeInput(dto);

            if (dto.AppointmentDate.Date < today)
                return this.ApiBadRequest("Chỉ được đặt lịch từ hôm nay trở đi");

            var localNow = DateTime.Now;
            if (dto.AppointmentDate.Date == localNow.Date && dto.AppointmentTime <= localNow.TimeOfDay)
                return this.ApiBadRequest("Chỉ được đặt lịch ở giờ tương lai");

            if (dto.AppointmentTime < businessStart || dto.AppointmentTime > businessEnd)
                return this.ApiBadRequest("Chỉ nhận đặt lịch 07:00 - 22:00");

            if (string.IsNullOrWhiteSpace(dto.Email))
                return this.ApiBadRequest("Email bắt buộc");

            Doctor? doctor = null;
            if (dto.DoctorId.HasValue && dto.DoctorId.Value != Guid.Empty)
            {
                doctor = await _context.Doctors
                    .AsNoTracking()
                    .Include(d => d.Department)
                    .FirstOrDefaultAsync(d => d.Id == dto.DoctorId.Value && d.Status != DoctorStatus.Inactive);

                if (doctor == null)
                    return this.ApiBadRequest("Bác sĩ không khả dụng");

                var slotError = await _appointmentBookingService.ValidateDoctorSlotAsync(
                    dto.DoctorId.Value,
                    dto.AppointmentDate.Date,
                    dto.AppointmentTime);
                if (slotError != null)
                    return this.ApiBadRequest(slotError);
            }

            if (!string.IsNullOrWhiteSpace(dto.CitizenId) &&
                (!dto.CitizenId.All(char.IsDigit) || dto.CitizenId.Length != 12))
            {
                return this.ApiBadRequest("CCCD phải gồm 12 chữ số");
            }

            var verified = await _appointmentBookingService.IsOtpVerifiedAsync(dto.Email);
            if (!verified)
                return this.ApiBadRequest("Email chưa xác thực OTP");

            var patient = await _appointmentBookingService.FindPatientAsync(dto);

            if (patient == null)
            {
                var patientCode = await _appointmentBookingService.GenerateUniquePatientCodeAsync();
                patient = _appointmentBookingService.BuildNewPatient(dto, patientCode);

                _context.Patients.Add(patient);
                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateException ex) when (
                    ex.InnerException?.Message.Contains("IX_Patients_InsuranceCardNumber") == true)
                {
                    return this.ApiConflict("Số thẻ BHYT này đã được đăng ký cho bệnh nhân khác trong hệ thống.", "insurance_duplicate");
                }
            }
            else
            {
                await _appointmentBookingService.EnsurePatientCodeAsync(patient);
                _appointmentBookingService.MergePatientProfile(patient, dto);
                await _context.SaveChangesAsync();
            }

            var existed = await _appointmentBookingService.HasExistingAppointmentAsync(
                patient.Id,
                dto.AppointmentDate.Date,
                dto.AppointmentTime);

            if (existed)
                return this.ApiConflict("Ban da dat lich gio nay roi", "appointment_exists");

            var code = await _appointmentBookingService.GenerateUniqueAppointmentCodeAsync();

            var appointment = new Appointment
            {
                Id = Guid.NewGuid(),
                PatientId = patient.Id,
                DoctorId = doctor?.Id,
                AppointmentCode = code,
                AppointmentDate = dto.AppointmentDate.Date,
                AppointmentTime = dto.AppointmentTime,
                Reason = dto.Reason,
                Status = doctor != null ? AppointmentStatus.Confirmed : AppointmentStatus.Pending
            };

            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();

            if (!string.IsNullOrWhiteSpace(dto.Email))
            {
                await _appointmentBookingService.SendAppointmentConfirmationAsync(
                    dto.Email,
                    patient.FullName,
                    appointment.AppointmentCode,
                    appointment.AppointmentDate,
                    appointment.AppointmentTime);
            }

            return Ok(ToDetailDto(appointment, patient, doctor));
        }

        [HttpGet("patient-lookup")]
        [AllowAnonymous]
        public async Task<IActionResult> LookupPatient([FromQuery] string? phone, [FromQuery] string? email)
        {
            if (string.IsNullOrWhiteSpace(phone) && string.IsNullOrWhiteSpace(email))
                return this.ApiBadRequest("Nhap SDT hoac email");

            var patient = await _context.Patients
                .AsNoTracking()
                .FirstOrDefaultAsync(p =>
                    (!string.IsNullOrWhiteSpace(phone) && p.Phone == phone) ||
                    (!string.IsNullOrWhiteSpace(email) && p.Email == email));

            if (patient == null)
                return this.ApiNotFound("Khong tim thay");

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
                .Include(a => a.Doctor)
                    .ThenInclude(d => d.Department)
                .FirstOrDefaultAsync(a => a.AppointmentCode == code);

            if (appointment == null || appointment.Patient == null)
                return this.ApiNotFound("Khong tim thay lich kham");

            return Ok(ToDetailDto(appointment, appointment.Patient, appointment.Doctor));
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
                return this.ApiBadRequest("Can nhap SDT, email, CCCD hoac BHYT");

            var query = _context.Appointments
                .Include(x => x.Patient)
                .Include(x => x.Doctor)
                    .ThenInclude(d => d.Department)
                .AsQueryable();

            query = query.Where(x =>
                (!string.IsNullOrWhiteSpace(dto.Phone) && x.Patient!.Phone == dto.Phone) ||
                (!string.IsNullOrWhiteSpace(dto.Email) && x.Patient!.Email == dto.Email) ||
                (!string.IsNullOrWhiteSpace(dto.CitizenId) && x.Patient!.CitizenId == dto.CitizenId) ||
                (!string.IsNullOrWhiteSpace(dto.InsuranceCardNumber) && x.Patient!.InsuranceCardNumber == dto.InsuranceCardNumber));

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
                .Select(a => ToDetailDto(a, a.Patient!, a.Doctor))
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
                    a.Patient!.FullName == dto.FullName &&
                    a.Patient.Phone == dto.Phone);

            if (appointment == null)
                return this.ApiNotFound("Khong tim thay lich kham");

            appointment.Status = AppointmentStatus.Cancelled;
            await _context.SaveChangesAsync();

            return Ok("Huy lich thanh cong");
        }

        [HttpGet("no-show")]
        public async Task<IActionResult> GetNoShowAppointments()
        {
            var data = await _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Doctor)
                    .ThenInclude(d => d.Department)
                .Where(a => a.Status == AppointmentStatus.NoShow)
                .OrderByDescending(a => a.AppointmentDate)
                .ToListAsync();

            var result = data
                .Where(a => a.Patient != null)
                .Select(a => ToDetailDto(a, a.Patient!, a.Doctor));

            return Ok(result);
        }

        [HttpGet("by-status")]
        public async Task<IActionResult> GetByStatus([FromQuery] AppointmentStatus status)
        {
            var data = await _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Doctor)
                    .ThenInclude(d => d.Department)
                .Where(a => a.Status == status)
                .OrderByDescending(a => a.AppointmentDate)
                .ToListAsync();

            var result = data
                .Where(a => a.Patient != null)
                .Select(a => ToDetailDto(a, a.Patient!, a.Doctor));

            return Ok(result);
        }

        private static AppointmentDetailDto ToDetailDto(Appointment appointment, Patient patient, Doctor? doctor = null)
        {
            var selectedDoctor = doctor ?? appointment.Doctor;

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
                InsuranceCardNumber = patient.InsuranceCardNumber,
                StatusDetail = new AppointmentStatusDto
                {
                    Value = appointment.Status.ToString(),
                    DoctorId = selectedDoctor?.Id,
                    DoctorName = selectedDoctor?.FullName,
                    DoctorCode = selectedDoctor?.Code,
                    DoctorDepartmentName = selectedDoctor?.Department?.Name ?? string.Empty
                }
            };
        }
    }
}
