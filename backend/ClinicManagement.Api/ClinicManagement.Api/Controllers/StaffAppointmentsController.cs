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
using System;
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

        public StaffAppointmentsController(ClinicDbContext context, FakeInsuranceService fakeInsuranceService, IConfiguration configuration)
        {
            _context = context;
            _fakeInsuranceService = fakeInsuranceService;
            _configuration = configuration;
        }

        [HttpPost("walk-in")]
        public async Task<IActionResult> CreateWalkInAppointment([FromBody] CreateAppointmentDto dto)
        {
            var today = DateTime.UtcNow.Date;
            var businessStart = new TimeSpan(7, 0, 0);
            var businessEnd = new TimeSpan(22, 0, 0);

            if (dto.AppointmentDate.Date < today)
                return BadRequest(new { message = "Chỉ được đặt lịch từ hôm nay trở đi" });

            if (dto.AppointmentTime < businessStart || dto.AppointmentTime > businessEnd)
                return BadRequest(new { message = "Chỉ nhận đặt lịch trong khung giờ làm việc (07:00-22:00)" });

            var patient = await _context.Patients
                .FirstOrDefaultAsync(p => p.Phone == dto.Phone && p.FullName == dto.FullName);

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
            else
            {
                patient.DateOfBirth = dto.DateOfBirth;
                patient.Gender = dto.Gender;
                patient.Address = string.IsNullOrWhiteSpace(dto.Address) ? patient.Address : dto.Address;
                patient.Email = string.IsNullOrWhiteSpace(dto.Email) ? patient.Email : dto.Email;
                patient.UpdatedAt = DateTime.UtcNow;
            }

            var existed = await _context.Appointments.AnyAsync(a =>
                a.PatientId == patient.Id &&
                a.AppointmentDate == dto.AppointmentDate.Date &&
                a.AppointmentTime == dto.AppointmentTime &&
                a.Status != AppointmentStatus.Cancelled);

            if (existed)
                return BadRequest(new { message = "Bệnh nhân đã có lịch ở khung giờ này" });

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
                AppointmentCode = code,
                AppointmentDate = dto.AppointmentDate.Date,
                AppointmentTime = dto.AppointmentTime,
                Reason = dto.Reason,
                Status = AppointmentStatus.Pending
            };

            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();

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
                CreatedAt = appointment.CreatedAt
            });
        }

        // GET: api/staff/Appointments
        [HttpGet]
        public async Task<ActionResult<IEnumerable<AppointmentDetailDto>>> GetAppointments()
        {
            var appointments = await _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Doctor)
      
    .ThenInclude(d => d.Department)

                .Select(a => new AppointmentDetailDto
                {
                    Id = a.Id,
                    AppointmentCode = a.AppointmentCode,
                    FullName = a.Patient != null ? a.Patient.FullName : null,
                    Phone = a.Patient != null ? a.Patient.Phone : null,
                    Email = a.Patient != null ? a.Patient.Email : null,
                    DateOfBirth = a.Patient != null ? a.Patient.DateOfBirth : DateTime.MinValue,
                    Gender = a.Patient != null ? a.Patient.Gender.ToString() : null,
                    Address = a.Patient != null ? a.Patient.Address : null,
                    Reason = a.Reason,
                    Status = a.Status.ToString(),
                    AppointmentDate = a.AppointmentDate,
                    AppointmentTime = a.AppointmentTime,
                    CreatedAt = a.CreatedAt,
                    StatusDetail = new AppointmentStatusDto
                    {
                        Value = a.Status.ToString(),
                        DoctorId = (a.Status == AppointmentStatus.Confirmed || a.Status == AppointmentStatus.Completed || a.Status == AppointmentStatus.CheckedIn) && a.Doctor != null
                            ? a.Doctor.Id
                            : null,
                        DoctorName = (a.Status == AppointmentStatus.Confirmed || a.Status == AppointmentStatus.Completed || a.Status == AppointmentStatus.CheckedIn) && a.Doctor != null
                            ? a.Doctor.FullName
                            : null,
                        DoctorCode = (a.Status == AppointmentStatus.Confirmed || a.Status == AppointmentStatus.Completed || a.Status == AppointmentStatus.CheckedIn) && a.Doctor != null
                            ? a.Doctor.Code
                            : null,
                        DoctorDepartmentName = (a.Status == AppointmentStatus.Confirmed || a.Status == AppointmentStatus.Completed || a.Status == AppointmentStatus.CheckedIn) && a.Doctor != null
                            ? a.Doctor.Department.Name
                            : null
                    }


                })
                .ToListAsync();

            return Ok(appointments);
        }


        [HttpGet("filter")]
        public async Task<ActionResult<IEnumerable<AppointmentDetailDto>>> GetAppointmentsByStatus([FromQuery] AppointmentStatus status)
        {
            var appointments = await _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Doctor)
                    .ThenInclude(d => d.User) // thêm include User
                .Where(a => a.Status == status)
                .Select(a => new AppointmentDetailDto
                {
                    Id = a.Id,
                    AppointmentCode = a.AppointmentCode,
                    FullName = a.Patient != null ? a.Patient.FullName : null,
                    Phone = a.Patient != null ? a.Patient.Phone : null,
                    Email = a.Patient != null ? a.Patient.Email : null,
                    DateOfBirth = a.Patient != null ? a.Patient.DateOfBirth : DateTime.MinValue,
                    Gender = a.Patient != null ? a.Patient.Gender.ToString() : null,
                    Address = a.Patient != null ? a.Patient.Address : null,
                    Reason = a.Reason,
                    AppointmentDate = a.AppointmentDate,
                    AppointmentTime = a.AppointmentTime,
                    CreatedAt = a.CreatedAt,

                    StatusDetail = new AppointmentStatusDto
                    {
                        Value = a.Status.ToString(),
                        DoctorId = (a.Status == AppointmentStatus.Confirmed || a.Status == AppointmentStatus.Completed || a.Status == AppointmentStatus.CheckedIn) && a.Doctor != null
                            ? a.Doctor.Id
                            : null,
                        DoctorName = (a.Status == AppointmentStatus.Confirmed || a.Status == AppointmentStatus.Completed || a.Status == AppointmentStatus.CheckedIn) && a.Doctor != null
                            ? a.Doctor.FullName
                            : null,
                        DoctorCode = (a.Status == AppointmentStatus.Confirmed || a.Status == AppointmentStatus.Completed || a.Status == AppointmentStatus.CheckedIn) && a.Doctor != null
                            ? a.Doctor.Code
                            : null,
                        DoctorDepartmentName = (a.Status == AppointmentStatus.Confirmed || a.Status == AppointmentStatus.Completed || a.Status == AppointmentStatus.CheckedIn) && a.Doctor != null
                            ? a.Doctor.Department.Name
                            : null
                    }

                })
                .ToListAsync();

            return Ok(appointments);
        }
        // StaffAppointmentsController.cs
        [HttpGet("specialties-by-department/{departmentId}")]
        public async Task<IActionResult> GetSpecialtiesByDepartment(Guid departmentId)
        {
            // giả sử bạn có DbSet<Specialty> với DepartmentId
            var specialties = await _context.Specialties
                .Where(s => s.DepartmentId == departmentId)
                .Select(s => new
                {
                    s.Id,
                    s.Name
                })
                .ToListAsync();

            return Ok(specialties);
        }
        [HttpGet("by-specialty/{specialtyId}")]
        public async Task<IActionResult> GetDoctorsBySpecialty(Guid specialtyId)
        {
            var doctors = await _context.Doctors
                .Include(d => d.Department)
                .Include(d => d.Specialty)
                .Where(d => d.SpecialtyId == specialtyId
                            && d.Status == DoctorStatus.Active)
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
       .FirstOrDefaultAsync(d => d.Id == dto.DoctorId
                              && d.Status == DoctorStatus.Active);

            if (doctor == null)
                return BadRequest("Doctor is not available");
            var isBusy = await _context.Appointments.AnyAsync(a =>
                a.DoctorId == dto.DoctorId &&
                a.AppointmentDate == appointment.AppointmentDate &&
                a.AppointmentTime == appointment.AppointmentTime &&
                (a.Status == AppointmentStatus.Confirmed || a.Status == AppointmentStatus.CheckedIn));

            if (isBusy)
                return BadRequest(new { message = "Doctor is already booked/checked-in at this time slot" });

            appointment.DoctorId = doctor.Id;
            appointment.Status = AppointmentStatus.Confirmed;

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
                .FirstOrDefaultAsync(a => a.Id == dto.AppointmentId);

            if (appointment == null) return NotFound("Appointment not found");
            if (appointment.Status == AppointmentStatus.Cancelled || appointment.Status == AppointmentStatus.Completed)
                return BadRequest("Cannot check-in cancelled/completed appointment");

            if (dto.DepositAmount > depositCap)
                return BadRequest($"Deposit cannot exceed {depositCap:N0} VND");

            // Optional assign doctor during check-in
            if (dto.DoctorId.HasValue)
            {
                var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.Id == dto.DoctorId && d.Status == DoctorStatus.Active);
                if (doctor == null) return BadRequest("Doctor is not available");
                appointment.DoctorId = doctor.Id;
            }
            else if (appointment.DoctorId == null)
            {
                return BadRequest("Please assign a doctor before check-in");
            }

            appointment.Status = AppointmentStatus.CheckedIn;
            appointment.CheckedInAt = DateTime.UtcNow;
            appointment.CheckInChannel = "Staff";

            // Lưu bảo hiểm vào hồ sơ khám (tạo mới nếu chưa có) để tính phí trước khi thu tạm ứng
            if (!string.IsNullOrWhiteSpace(dto.InsuranceCode) || (dto.InsuranceCoverPercent ?? 0) > 0)
            {
                var plan = !string.IsNullOrWhiteSpace(dto.InsuranceCode)
                    ? _fakeInsuranceService.Verify(dto.InsuranceCode!)
                    : null;

                if (!string.IsNullOrWhiteSpace(dto.InsuranceCode) && plan == null)
                {
                    return BadRequest("Mã bảo hiểm không hợp lệ hoặc đã hết hạn");
                }

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
            // create deposit only for inpatient
            if (dto.IsInpatient && dto.DepositAmount > 0)
            {
                // nếu đã có invoice cho appointment thì gắn luôn
                var invoice = await _context.Invoices.FirstOrDefaultAsync(i => i.AppointmentId == appointment.Id);

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
                    .ThenInclude(d => d.User)
                .Include(a => a.Doctor)
                    .ThenInclude(d => d.Department)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (appointment == null) return NotFound();

            var dto = new AppointmentDetailDto
            {
                Id = appointment.Id,
                AppointmentCode = appointment.AppointmentCode,
                FullName = appointment.Patient?.FullName ?? string.Empty,
                Phone = appointment.Patient?.Phone ?? string.Empty,
                Email = appointment.Patient?.Email,
                DateOfBirth = appointment.Patient?.DateOfBirth ?? DateTime.MinValue,
                Gender = appointment.Patient?.Gender.ToString() ?? string.Empty,
                Address = appointment.Patient?.Address,
                Reason = appointment.Reason,
                Status = appointment.Status.ToString(),
                AppointmentDate = appointment.AppointmentDate,
                AppointmentTime = appointment.AppointmentTime,
                CreatedAt = appointment.CreatedAt,
                StatusDetail = new AppointmentStatusDto
                {
                    Value = appointment.Status.ToString(),
                    DoctorName = (appointment.Status == AppointmentStatus.Confirmed || appointment.Status == AppointmentStatus.Completed || appointment.Status == AppointmentStatus.CheckedIn)
                        ? appointment.Doctor?.FullName
                        : null,
                    DoctorCode = (appointment.Status == AppointmentStatus.Confirmed || appointment.Status == AppointmentStatus.Completed || appointment.Status == AppointmentStatus.CheckedIn)
                        ? appointment.Doctor?.Code
                        : null,
                    DoctorDepartmentName = (appointment.Status == AppointmentStatus.Confirmed || appointment.Status == AppointmentStatus.Completed || appointment.Status == AppointmentStatus.CheckedIn)
                        ? appointment.Doctor?.Department?.Name
                        : null
                }
            };

            return Ok(dto);
        }



    }
}
