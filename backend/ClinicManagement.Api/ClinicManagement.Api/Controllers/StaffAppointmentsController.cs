using ClinicManagement.Api.Data;
using ClinicManagement.Api.Dtos.Appointments;
using ClinicManagement.Api.DTOs.Appointments;
using ClinicManagement.Api.Extensions;
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
        private readonly AppointmentBookingService _appointmentBookingService;

        public StaffAppointmentsController(
            ClinicDbContext context,
            FakeInsuranceService fakeInsuranceService,
            IConfiguration configuration,
            DoctorScheduleService doctorScheduleService,
            AppointmentBookingService appointmentBookingService)
        {
            _context = context;
            _fakeInsuranceService = fakeInsuranceService;
            _configuration = configuration;
            _doctorScheduleService = doctorScheduleService;
            _appointmentBookingService = appointmentBookingService;
        }

        [HttpPost("walk-in")]
        public async Task<IActionResult> CreateWalkInAppointment([FromBody] CreateAppointmentDto dto)
        {
            // Use local date consistently (avoid allowing "yesterday" bookings around midnight due to UTC).
            var today = DateTime.Today;
            var businessStart = new TimeSpan(7, 0, 0);
            var businessEnd = new TimeSpan(22, 0, 0);

            _appointmentBookingService.NormalizeInput(dto);

            if (dto.AppointmentDate.Date < today)
                return this.ApiBadRequest("Chi duoc dat lich tu hom nay tro di");

            var localNow = DateTime.Now;
            if (dto.AppointmentDate.Date == localNow.Date && dto.AppointmentTime <= localNow.TimeOfDay)
                return this.ApiBadRequest("Chi duoc dat lich o gio tuong lai");

            if (dto.AppointmentTime < businessStart || dto.AppointmentTime > businessEnd)
                return this.ApiBadRequest("Chi nhan dat lich trong khung gio 07:00-22:00");

            Doctor? doctor = null;
            if (dto.DoctorId.HasValue && dto.DoctorId.Value != Guid.Empty)
            {
                doctor = await _context.Doctors
                    .AsNoTracking()
                    .Include(d => d.Department)
                    .FirstOrDefaultAsync(d => d.Id == dto.DoctorId.Value && d.Status != DoctorStatus.Inactive);

                if (doctor == null)
                    return this.ApiBadRequest("Bac si khong kha dung");

                var slotError = await _appointmentBookingService.ValidateDoctorSlotAsync(
                    dto.DoctorId.Value,
                    dto.AppointmentDate.Date,
                    dto.AppointmentTime);
                if (slotError != null)
                    return this.ApiBadRequest(slotError);
            }

            var patient = await _appointmentBookingService.FindPatientAsync(dto);

            if (patient == null)
            {
                var patientCode = await _appointmentBookingService.GenerateUniquePatientCodeAsync();
                patient = _appointmentBookingService.BuildNewPatient(dto, patientCode);

                _context.Patients.Add(patient);
            }
            else
            {
                await _appointmentBookingService.EnsurePatientCodeAsync(patient);
                _appointmentBookingService.MergePatientProfile(patient, dto);
            }

            var existed = await _appointmentBookingService.HasExistingAppointmentAsync(
                patient.Id,
                dto.AppointmentDate.Date,
                dto.AppointmentTime);

            if (existed)
                return this.ApiConflict("Benh nhan da co lich o khung gio nay", "appointment_exists");

            var code = await _appointmentBookingService.GenerateUniqueAppointmentCodeAsync();

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
                DoctorId = doctor?.Id,
                AppointmentCode = code,
                AppointmentDate = dto.AppointmentDate.Date,
                AppointmentTime = dto.AppointmentTime,
                Reason = dto.Reason,
                Status = doctor != null ? AppointmentStatus.Confirmed : AppointmentStatus.Pending
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
                return this.ApiNotFound("Appointment not found");

            var doctor = await _context.Doctors
                .AsNoTracking()
                .Include(d => d.Department)
                .FirstOrDefaultAsync(d => d.Id == dto.DoctorId && d.Status != DoctorStatus.Inactive);

            if (doctor == null)
                return this.ApiBadRequest("Doctor is not available");

            var slotError = await _appointmentBookingService.ValidateDoctorSlotAsync(
                dto.DoctorId,
                appointment.AppointmentDate,
                appointment.AppointmentTime,
                appointment.Id);

            if (slotError != null)
                return this.ApiBadRequest(slotError);

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
                return this.ApiNotFound("Appointment not found");

            if (appointment.Patient != null && string.IsNullOrWhiteSpace(appointment.Patient.PatientCode))
            {
                appointment.Patient.PatientCode =
                    await _appointmentBookingService.GenerateUniquePatientCodeAsync();
            }

            if (appointment.Status == AppointmentStatus.Cancelled || appointment.Status == AppointmentStatus.Completed)
                return this.ApiBadRequest("Cannot check-in cancelled/completed appointment");
            if (appointment.Status == AppointmentStatus.CheckedIn)
                return this.ApiConflict("Appointment already checked in", "already_checked_in");

            if (dto.DepositAmount > depositCap)
                return this.ApiBadRequest($"Deposit cannot exceed {depositCap:N0} VND");

            if (dto.DoctorId.HasValue)
            {
                var doctor = await _context.Doctors
                    .AsNoTracking()
                    .FirstOrDefaultAsync(d => d.Id == dto.DoctorId.Value && d.Status != DoctorStatus.Inactive);

                if (doctor == null)
                    return this.ApiBadRequest("Doctor is not available");

                var slotError = await _appointmentBookingService.ValidateDoctorSlotAsync(
                    dto.DoctorId.Value,
                    appointment.AppointmentDate,
                    appointment.AppointmentTime,
                    appointment.Id);

                if (slotError != null)
                    return this.ApiBadRequest(slotError);

                appointment.DoctorId = doctor.Id;
            }
            else if (appointment.DoctorId == null && dto.RoomId.HasValue && dto.RoomId.Value != Guid.Empty)
            {
                var roomSlot = await _doctorScheduleService.GetEffectiveSlotByRoomAsync(
                    dto.RoomId.Value,
                    appointment.AppointmentDate,
                    appointment.AppointmentTime);

                if (roomSlot == null)
                    return this.ApiBadRequest("Selected room does not have a scheduled doctor on duty at this slot");

                appointment.DoctorId = roomSlot.DoctorId;
            }
            else if (appointment.DoctorId == null)
            {
                return this.ApiBadRequest("Please select a room with doctor on duty before check-in");
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
                    return this.ApiBadRequest("Ma bao hiem khong hop le hoac da het han");

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
            var existingDeposit = await _context.Payments
                .Where(p => p.AppointmentId == appointment.Id && p.IsDeposit)
                .SumAsync(p => p.Amount);

            if (dto.IsInpatient && dto.DepositAmount > 0)
            {
                var remainingDeposit = dto.DepositAmount - existingDeposit;

                if (remainingDeposit <= 0)
                {
                    await _context.SaveChangesAsync();

                    return Ok(new
                    {
                        message = "Checked in successfully",
                        appointment.Status,
                        totalDeposit = existingDeposit,
                        depositPaymentId = (Guid?)null
                    });
                }

                var invoice = await _context.Invoices
                    .FirstOrDefaultAsync(i => i.AppointmentId == appointment.Id);

                depositPayment = new Payment
                {
                    Id = Guid.NewGuid(),
                    AppointmentId = appointment.Id,
                    InvoiceId = invoice?.Id,
                    Amount = remainingDeposit,
                    DepositAmount = remainingDeposit,
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
                return NotFound(new ApiErrorResponse { Code = "not_found", Message = "Appointment not found" });

            return Ok(ToDetailDto(appointment, appointment.Patient, appointment.Doctor));
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
