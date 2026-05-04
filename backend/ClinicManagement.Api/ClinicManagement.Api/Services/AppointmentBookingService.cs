using ClinicManagement.Api.Data;
using ClinicManagement.Api.DTOs.Appointments;
using ClinicManagement.Api.Models;
using ClinicManagement.Api.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ClinicManagement.Api.Services
{
    public class AppointmentBookingService
    {
        private readonly ClinicDbContext _context;
        private readonly DoctorScheduleService _doctorScheduleService;
        private readonly OtpService _otpService;
        private readonly EmailService _emailService;
        private readonly ILogger<AppointmentBookingService> _logger;

        public AppointmentBookingService(
            ClinicDbContext context,
            DoctorScheduleService doctorScheduleService,
            OtpService otpService,
            EmailService emailService,
            ILogger<AppointmentBookingService> logger)
        {
            _context = context;
            _doctorScheduleService = doctorScheduleService;
            _otpService = otpService;
            _emailService = emailService;
            _logger = logger;
        }

        public void NormalizeInput(CreateAppointmentDto dto)
        {
            dto.CitizenId = dto.CitizenId?.Trim();
            dto.Phone = dto.Phone?.Trim();
            dto.FullName = dto.FullName?.Trim();
            dto.Email = dto.Email?.Trim();
            dto.InsuranceCardNumber = dto.InsuranceCardNumber?.Trim();
        }

        public async Task<Doctor?> GetAvailableDoctorAsync(Guid doctorId)
        {
            return await _context.Doctors
                .AsNoTracking()
                .Include(d => d.Department)
                .FirstOrDefaultAsync(d => d.Id == doctorId && d.Status != DoctorStatus.Inactive);
        }

        public async Task<string?> ValidateDoctorSlotAsync(
            Guid doctorId,
            DateTime appointmentDate,
            TimeSpan appointmentTime,
            Guid? currentAppointmentId = null)
        {
            var hasSchedule = await _doctorScheduleService.HasEffectiveSlotAsync(doctorId, appointmentDate, appointmentTime);
            if (!hasSchedule)
            {
                return "Doctor does not have a working slot at this time";
            }

            var isBusy = await _context.Appointments.AnyAsync(a =>
                a.Id != currentAppointmentId &&
                a.DoctorId == doctorId &&
                a.AppointmentDate == appointmentDate &&
                a.AppointmentTime == appointmentTime &&
                a.Status != AppointmentStatus.Cancelled &&
                a.Status != AppointmentStatus.NoShow);

            if (isBusy)
            {
                return "Doctor is already booked at this slot";
            }

            return null;
        }

        public async Task<Patient?> FindPatientAsync(CreateAppointmentDto dto)
        {
            if (!string.IsNullOrWhiteSpace(dto.CitizenId))
            {
                var byCitizenId = await _context.Patients
                    .FirstOrDefaultAsync(p => p.CitizenId == dto.CitizenId);
                if (byCitizenId != null)
                {
                    return byCitizenId;
                }
            }

            return await _context.Patients
                .FirstOrDefaultAsync(p => p.Phone == dto.Phone && p.FullName == dto.FullName);
        }

        public async Task EnsurePatientCodeAsync(Patient patient)
        {
            if (!string.IsNullOrWhiteSpace(patient.PatientCode))
            {
                return;
            }

            patient.PatientCode = await GenerateUniquePatientCodeAsync();
        }

        public Patient BuildNewPatient(CreateAppointmentDto dto, string patientCode)
        {
            return new Patient
            {
                Id = Guid.NewGuid(),
                PatientCode = patientCode,
                FullName = dto.FullName!,
                Phone = dto.Phone!,
                Email = dto.Email,
                Address = dto.Address,
                DateOfBirth = dto.DateOfBirth,
                Gender = dto.Gender,
                CitizenId = dto.CitizenId,
                InsuranceCardNumber = dto.InsuranceCardNumber
            };
        }

        public void MergePatientProfile(Patient patient, CreateAppointmentDto dto)
        {
            patient.DateOfBirth = dto.DateOfBirth;
            patient.Gender = dto.Gender;
            patient.Address = string.IsNullOrWhiteSpace(dto.Address) ? patient.Address : dto.Address;
            patient.Email = string.IsNullOrWhiteSpace(dto.Email) ? patient.Email : dto.Email;
            patient.UpdatedAt = DateTime.UtcNow;

            if (string.IsNullOrWhiteSpace(patient.CitizenId))
            {
                patient.CitizenId = dto.CitizenId;
            }

            if (string.IsNullOrWhiteSpace(patient.InsuranceCardNumber))
            {
                patient.InsuranceCardNumber = dto.InsuranceCardNumber;
            }
        }

        public async Task<bool> HasExistingAppointmentAsync(Guid patientId, DateTime appointmentDate, TimeSpan appointmentTime)
        {
            return await _context.Appointments.AnyAsync(a =>
                a.PatientId == patientId &&
                a.AppointmentDate == appointmentDate.Date &&
                a.AppointmentTime == appointmentTime &&
                a.Status != AppointmentStatus.Cancelled);
        }

        public async Task<string> GenerateUniquePatientCodeAsync()
        {
            string patientCode;
            do
            {
                patientCode = CodeGenerator.GeneratePatientCode();
            }
            while (await _context.Patients.AnyAsync(p => p.PatientCode == patientCode));

            return patientCode;
        }

        public async Task<string> GenerateUniqueAppointmentCodeAsync()
        {
            string code;
            do
            {
                code = CodeGenerator.GenerateAppointmentCode();
            }
            while (await _context.Appointments.AnyAsync(a => a.AppointmentCode == code));

            return code;
        }

        public Task<bool> IsOtpVerifiedAsync(string email)
            => _otpService.IsVerifiedAsync(email);

        public async Task SendAppointmentConfirmationAsync(string toEmail, string patientName, string appointmentCode, DateTime appointmentDate, TimeSpan appointmentTime)
        {
            try
            {
                await _emailService.SendAsync(
                    toEmail,
                    "Xac nhan lich kham",
                    $"Ma kham: <b>{appointmentCode}</b><br/>Ho ten: {patientName}<br/>Ngay: {appointmentDate:dd/MM/yyyy} - Gio: {appointmentTime}");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to send appointment confirmation email.");
            }
        }
    }
}
