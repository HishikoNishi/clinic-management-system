using ClinicManagement.Api.Data;
using ClinicManagement.Api.Dtos.Appointments;
using ClinicManagement.Api.DTOs.Appointments;
using ClinicManagement.Api.Models;
using ClinicManagement.Api.Services;
using ClinicManagement.Api.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Security.Claims;

namespace ClinicManagement.Api.Controllers
{
    [ApiController]
    [Route("api/staff/[controller]")]
    [Authorize(Roles = "Admin,Staff")]
    public class StaffAppointmentsController : ControllerBase
    {
        private readonly ClinicDbContext _context;
        private readonly FakeInsuranceService _fakeInsuranceService;
        private readonly IConfiguration _configuration;
        private readonly DoctorScheduleService _doctorScheduleService;

        public StaffAppointmentsController(
            ClinicDbContext context,
            FakeInsuranceService fakeInsuranceService,
            IConfiguration configuration,
            DoctorScheduleService doctorScheduleService)
        {
            _context = context;
            _fakeInsuranceService = fakeInsuranceService;
            _configuration = configuration;
            _doctorScheduleService = doctorScheduleService;
        }

        [HttpPost("walk-in")]
        public async Task<IActionResult> CreateWalkInAppointment([FromBody] CreateAppointmentDto dto)
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
                return BadRequest(new { message = "Chi duoc dat lich tu hom nay tro di" });

            var localNow = DateTime.Now;
            if (dto.AppointmentDate.Date == localNow.Date && dto.AppointmentTime <= localNow.TimeOfDay)
                return BadRequest(new { message = "Chi duoc dat lich o gio tuong lai" });

            if (dto.AppointmentTime < businessStart || dto.AppointmentTime > businessEnd)
                return BadRequest(new { message = "Chi nhan dat lich trong khung gio 07:00-22:00" });

            if (dto.DoctorId == Guid.Empty)
                return BadRequest(new { message = "Vui long chon bac si" });

            var doctor = await _context.Doctors
                .AsNoTracking()
                .Include(d => d.Department)
                .FirstOrDefaultAsync(d => d.Id == dto.DoctorId && d.Status == DoctorStatus.Active);

            if (doctor == null)
                return BadRequest(new { message = "Bac si khong kha dung" });

            var slotError = await ValidateDoctorSlotAsync(dto.DoctorId, dto.AppointmentDate.Date, dto.AppointmentTime);
            if (slotError != null)
                return BadRequest(new { message = slotError });

            var patient = await _context.Patients
                .FirstOrDefaultAsync(p => p.Phone == dto.Phone && p.FullName == dto.FullName);

            if (patient == null)
            {
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
                    FullName = dto.FullName!,
                    Phone = dto.Phone!,
                    Email = dto.Email,
                    Address = dto.Address,
                    DateOfBirth = dto.DateOfBirth,
                    Gender = dto.Gender,
                    CitizenId = dto.CitizenId,
                    InsuranceCardNumber = dto.InsuranceCardNumber
                };

                _context.Patients.Add(patient);
            }
            else
            {
                patient.DateOfBirth = dto.DateOfBirth;
                patient.Gender = dto.Gender;
                patient.Address = string.IsNullOrWhiteSpace(dto.Address) ? patient.Address : dto.Address;
                patient.Email = string.IsNullOrWhiteSpace(dto.Email) ? patient.Email : dto.Email;
                patient.UpdatedAt = DateTime.UtcNow;

                if (string.IsNullOrWhiteSpace(patient.CitizenId))
                    patient.CitizenId = dto.CitizenId;

                if (string.IsNullOrWhiteSpace(patient.InsuranceCardNumber))
                    patient.InsuranceCardNumber = dto.InsuranceCardNumber;
            }

            var existed = await _context.Appointments.AnyAsync(a =>
                a.PatientId == patient.Id &&
                a.AppointmentDate == dto.AppointmentDate.Date &&
                a.AppointmentTime == dto.AppointmentTime &&
                a.Status != AppointmentStatus.Cancelled);

            if (existed)
                return BadRequest(new { message = "Benh nhan da co lich o khung gio nay" });

            string code;
            do
            {
                code = CodeGenerator.GenerateAppointmentCode();
            }
            while (await _context.Appointments.AnyAsync(a => a.AppointmentCode == code));

            Guid? staffId = null;
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (Guid.TryParse(userIdClaim, out var userId))
            {
                staffId = await _context.Staffs
                    .Where(s => s.UserId == userId)
                    .Select(s => (Guid?)s.Id)
                    .FirstOrDefaultAsync();
            }

            var appointment = new Appointment
            {
                Id = Guid.NewGuid(),
                PatientId = patient.Id,
                StaffId = staffId,
                DoctorId = doctor.Id,
                AppointmentCode = code,
                AppointmentDate = dto.AppointmentDate.Date,
                AppointmentTime = dto.AppointmentTime,
                Reason = dto.Reason,
                Status = AppointmentStatus.Confirmed
            };

            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();

            return Ok(ToDetailDto(appointment, patient, doctor));
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<AppointmentDetailDto>>> GetAppointments()
        {
            var appointments = await _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Doctor)
                    .ThenInclude(d => d.Department)
                .ToListAsync();

            return Ok(appointments
                .Where(a => a.Patient != null)
                .Select(a => ToDetailDto(a, a.Patient!, a.Doctor)));
        }

        [HttpGet("filter")]
        public async Task<ActionResult<IEnumerable<AppointmentDetailDto>>> GetAppointmentsByStatus([FromQuery] AppointmentStatus status)
        {
            var appointments = await _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Doctor)
                    .ThenInclude(d => d.Department)
                .Where(a => a.Status == status)
                .ToListAsync();

            return Ok(appointments
                .Where(a => a.Patient != null)
                .Select(a => ToDetailDto(a, a.Patient!, a.Doctor)));
        }

        [HttpGet("specialties-by-department/{departmentId}")]
        public async Task<IActionResult> GetSpecialtiesByDepartment(Guid departmentId)
        {
            var specialties = await _context.Specialties
                .Where(s => s.DepartmentId == departmentId)
                .Select(s => new { s.Id, s.Name })
                .ToListAsync();

            return Ok(specialties);
        }

        [HttpGet("by-specialty/{specialtyId}")]
        public async Task<IActionResult> GetDoctorsBySpecialty(Guid specialtyId)
        {
            var doctors = await _context.Doctors
                .Include(d => d.Department)
                .Include(d => d.Specialty)
                .Where(d => d.SpecialtyId == specialtyId && d.Status == DoctorStatus.Active)
                .Select(d => new
                {
                    d.Id,
                    d.FullName,
                    d.Code,
                    d.SpecialtyId,
                    SpecialtyName = d.Specialty.Name,
                    d.DepartmentId,
                    DepartmentName = d.Department.Name
                })
                .ToListAsync();

            return Ok(doctors);
        }

        [HttpPost("assign-doctor")]
        public async Task<IActionResult> AssignDoctor([FromBody] AssignDoctorDto dto)
        {
            var appointment = await _context.Appointments
                .FirstOrDefaultAsync(a => a.Id == dto.AppointmentId);

            if (appointment == null)
                return NotFound("Appointment not found");

            var doctor = await _context.Doctors
                .AsNoTracking()
                .Include(d => d.Department)
                .FirstOrDefaultAsync(d => d.Id == dto.DoctorId && d.Status == DoctorStatus.Active);

            if (doctor == null)
                return BadRequest("Doctor is not available");

            var slotError = await ValidateDoctorSlotAsync(
                dto.DoctorId,
                appointment.AppointmentDate,
                appointment.AppointmentTime,
                appointment.Id);

            if (slotError != null)
                return BadRequest(new { message = slotError });

            appointment.DoctorId = doctor.Id;
            if (appointment.Status != AppointmentStatus.CheckedIn)
            {
                appointment.Status = AppointmentStatus.Confirmed;
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Assigned doctor successfully",
                doctorCode = doctor.Code
            });
        }

        [HttpPost("checkin")]
        public async Task<IActionResult> CheckIn([FromBody] CheckInRequestDto dto)
        {
            var depositCap = _configuration.GetValue<decimal?>("Billing:DepositCap") ?? 200000m;

            var appointment = await _context.Appointments
                .Include(a => a.Patient)
                .FirstOrDefaultAsync(a => a.Id == dto.AppointmentId);

            if (appointment == null)
                return NotFound("Appointment not found");

            if (appointment.Patient != null && string.IsNullOrWhiteSpace(appointment.Patient.PatientCode))
            {
                string patientCode;
                do
                {
                    patientCode = CodeGenerator.GeneratePatientCode();
                }
                while (await _context.Patients.AnyAsync(p => p.PatientCode == patientCode));

                appointment.Patient.PatientCode = patientCode;
            }

            if (appointment.Status == AppointmentStatus.Cancelled || appointment.Status == AppointmentStatus.Completed)
                return BadRequest("Cannot check-in cancelled/completed appointment");

            if (dto.DepositAmount > depositCap)
                return BadRequest($"Deposit cannot exceed {depositCap:N0} VND");

            if (dto.DoctorId.HasValue)
            {
                var doctor = await _context.Doctors
                    .AsNoTracking()
                    .FirstOrDefaultAsync(d => d.Id == dto.DoctorId.Value && d.Status == DoctorStatus.Active);

                if (doctor == null)
                    return BadRequest("Doctor is not available");

                var slotError = await ValidateDoctorSlotAsync(
                    dto.DoctorId.Value,
                    appointment.AppointmentDate,
                    appointment.AppointmentTime,
                    appointment.Id);

                if (slotError != null)
                    return BadRequest(slotError);

                appointment.DoctorId = doctor.Id;
            }
            else if (appointment.DoctorId == null)
            {
                return BadRequest("Please assign a doctor before check-in");
            }

            appointment.Status = AppointmentStatus.CheckedIn;
            appointment.CheckedInAt = DateTime.UtcNow;
            appointment.CheckInChannel = "Staff";

            if (!string.IsNullOrWhiteSpace(dto.InsuranceCode) || (dto.InsuranceCoverPercent ?? 0) > 0)
            {
                var plan = !string.IsNullOrWhiteSpace(dto.InsuranceCode)
                    ? _fakeInsuranceService.Verify(dto.InsuranceCode!)
                    : null;

                if (!string.IsNullOrWhiteSpace(dto.InsuranceCode) && plan == null)
                    return BadRequest("Ma bao hiem khong hop le hoac da het han");

                var cover = dto.InsuranceCoverPercent ?? plan?.CoveragePercent ?? 0m;
                cover = FinanceHelper.Clamp01(cover);

                var medicalRecord = await _context.MedicalRecords
                    .FirstOrDefaultAsync(r => r.AppointmentId == appointment.Id);

                if (medicalRecord == null)
                {
                    medicalRecord = new MedicalRecord
                    {
                        Id = Guid.NewGuid(),
                        AppointmentId = appointment.Id,
                        DoctorId = appointment.DoctorId!.Value,
                        PatientId = appointment.PatientId,
                        Symptoms = appointment.Reason ?? string.Empty,
                        Diagnosis = string.Empty,
                        Treatment = string.Empty,
                        Note = string.Empty,
                        CreatedAt = DateTime.UtcNow
                    };
                    _context.MedicalRecords.Add(medicalRecord);
                }

                medicalRecord.InsurancePlanCode = plan?.Code ?? dto.InsuranceCode;
                medicalRecord.InsuranceCoverPercent = cover;
            }

            Payment? depositPayment = null;
            if (dto.IsInpatient && dto.DepositAmount > 0)
            {
                var invoice = await _context.Invoices
                    .FirstOrDefaultAsync(i => i.AppointmentId == appointment.Id);

                depositPayment = new Payment
                {
                    AppointmentId = appointment.Id,
                    InvoiceId = invoice?.Id,
                    Amount = dto.DepositAmount,
                    DepositAmount = dto.DepositAmount,
                    IsDeposit = true,
                    Method = dto.Method,
                    PaymentDate = DateTime.UtcNow
                };

                _context.Payments.Add(depositPayment);
            }

            await _context.SaveChangesAsync();

            var totalDeposit = await _context.Payments
                .Where(p => p.AppointmentId == appointment.Id && p.IsDeposit)
                .SumAsync(p => p.Amount);

            return Ok(new
            {
                message = "Checked in successfully",
                appointment.Status,
                totalDeposit,
                depositPaymentId = depositPayment?.Id
            });
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<AppointmentDetailDto>> GetAppointmentDetail(Guid id)
        {
            var appointment = await _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Doctor)
                    .ThenInclude(d => d.Department)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (appointment == null || appointment.Patient == null)
                return NotFound();

            return Ok(ToDetailDto(appointment, appointment.Patient, appointment.Doctor));
        }

        private async Task<string?> ValidateDoctorSlotAsync(
            Guid doctorId,
            DateTime appointmentDate,
            TimeSpan appointmentTime,
            Guid? currentAppointmentId = null)
        {
            var hasSchedule = await _doctorScheduleService.HasEffectiveSlotAsync(doctorId, appointmentDate, appointmentTime);

            if (!hasSchedule)
                return "Doctor does not have a working slot at this time";

            var isBusy = await _context.Appointments.AnyAsync(a =>
                a.Id != currentAppointmentId &&
                a.DoctorId == doctorId &&
                a.AppointmentDate == appointmentDate &&
                a.AppointmentTime == appointmentTime &&
                a.Status != AppointmentStatus.Cancelled &&
                a.Status != AppointmentStatus.NoShow);

            if (isBusy)
                return "Doctor is already booked at this slot";

            return null;
        }

        private static AppointmentDetailDto ToDetailDto(Appointment appointment, Patient patient, Doctor? doctor = null)
        {
            var selectedDoctor = doctor ?? appointment.Doctor;

            return new AppointmentDetailDto
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
                InsuranceCardNumber = patient.InsuranceCardNumber,
                Note = patient.Note ?? string.Empty,
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
