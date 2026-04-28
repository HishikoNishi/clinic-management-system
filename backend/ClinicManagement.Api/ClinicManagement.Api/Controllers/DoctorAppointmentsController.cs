using ClinicManagement.Api.Data;
using ClinicManagement.Api.Dtos.Appointments;
using ClinicManagement.Api.Dtos.MedicalRecords;
using ClinicManagement.Api.DTOs.Appointments;
using ClinicManagement.Api.Models;
using ClinicManagement.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ClinicManagement.Api.Controllers
{
    [ApiController]
    [Route("api/doctor/[controller]")]
    [Authorize(Roles = "Doctor")]
    public class DoctorAppointmentsController : ControllerBase
    {
        private readonly ClinicDbContext _context;
        private readonly AppointmentBookingService _appointmentBookingService;
        private readonly DoctorScheduleService _doctorScheduleService;

        public DoctorAppointmentsController(
            ClinicDbContext context,
            AppointmentBookingService appointmentBookingService,
            DoctorScheduleService doctorScheduleService)
        {
            _context = context;
            _appointmentBookingService = appointmentBookingService;
            _doctorScheduleService = doctorScheduleService;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<AppointmentDetailDto>> GetAppointmentDetail(Guid id)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdClaim, out Guid userId))
                return Unauthorized("Invalid user id in token");

            var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.UserId == userId);
            if (doctor == null) return Unauthorized("Doctor not found");

            var appointment = await _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Doctor).ThenInclude(d => d.User)
                .FirstOrDefaultAsync(a => a.Id == id && a.DoctorId == doctor.Id);

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
                PatientCode = appointment.Patient?.PatientCode,
                CitizenId = appointment.Patient?.CitizenId,
                InsuranceCardNumber = appointment.Patient?.InsuranceCardNumber,

                StatusDetail = new AppointmentStatusDto
                {
                    Value = appointment.Status.ToString(),
                    DoctorName = (appointment.Status == AppointmentStatus.Confirmed || appointment.Status == AppointmentStatus.Completed)
                        ? appointment.Doctor?.User?.Username
                        : null,
                    DoctorCode = (appointment.Status == AppointmentStatus.Confirmed || appointment.Status == AppointmentStatus.Completed)
                        ? appointment.Doctor?.Code
                        : null
                }
            };

            return Ok(dto);
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<AppointmentDetailDto>>> GetAppointmentsForDoctor([FromQuery] string? status = null)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdClaim, out Guid userId))
                return Unauthorized("Invalid user id in token");

            var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.UserId == userId);
            if (doctor == null) return Unauthorized("Doctor not found");

            var query = _context.Appointments
                .Include(a => a.Patient)
                .Where(a => a.DoctorId == doctor.Id);

            if (!string.IsNullOrWhiteSpace(status))
            {
                var statusList = status.Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => s.Trim())
                    .Select(s => Enum.TryParse<AppointmentStatus>(s, true, out var parsed) ? parsed : (AppointmentStatus?)null)
                    .Where(s => s.HasValue).Select(s => s!.Value).ToList();
                if (statusList.Any()) query = query.Where(a => statusList.Contains(a.Status));
            }

            var appointments = await query.Select(a => new AppointmentDetailDto
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
                PatientCode = a.Patient != null ? a.Patient.PatientCode : null,
                CitizenId = a.Patient != null ? a.Patient.CitizenId : null,
                InsuranceCardNumber = a.Patient != null ? a.Patient.InsuranceCardNumber : null,
                StatusDetail = new AppointmentStatusDto
                {
                    Value = a.Status.ToString(),
                    DoctorName = (a.Status == AppointmentStatus.Confirmed || a.Status == AppointmentStatus.Completed) && a.Doctor != null
                        ? a.Doctor.User.Username
                        : null,
                    DoctorCode = (a.Status == AppointmentStatus.Confirmed || a.Status == AppointmentStatus.Completed) && a.Doctor != null
                        ? a.Doctor.Code
                        : null
                }
            }).ToListAsync();

            return Ok(appointments);
        }

        [HttpGet("{id}/examination")]
        public async Task<ActionResult<ExaminationDetailDto>> GetExaminationDetail(Guid id)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdClaim, out Guid userId))
                return Unauthorized("Invalid user id in token");

            var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.UserId == userId);
            if (doctor == null) return Unauthorized("Doctor not found");

            var appointment = await _context.Appointments
                .Include(a => a.Patient)
                .FirstOrDefaultAsync(a => a.Id == id && a.DoctorId == doctor.Id);
            if (appointment == null) return NotFound();

            var history = await _context.MedicalRecords
                .Where(r => r.PatientId == appointment.PatientId)
                .OrderByDescending(r => r.CreatedAt)
                .Take(5)
                .Select(r => new BasicMedicalHistoryDto
                {
                    Id = r.Id,
                    CreatedAt = r.CreatedAt,
                    Diagnosis = r.Diagnosis,
                    Note = r.Note
                })
                .ToListAsync();

            var currentRecord = await _context.MedicalRecords
                .Where(r => r.AppointmentId == appointment.Id)
                .OrderByDescending(r => r.CreatedAt)
                .FirstOrDefaultAsync();

            var prescriptionItems = new List<PrescriptionItemDto>();
            var clinicalTests = new List<ClinicManagement.Api.Dtos.ClinicalTests.ClinicalTestDto>();
            if (currentRecord != null)
            {
                var pres = await _context.Prescriptions
                    .Include(p => p.PrescriptionDetails)
                    .FirstOrDefaultAsync(p => p.MedicalRecordId == currentRecord.Id);
                if (pres?.PrescriptionDetails != null)
                {
                    prescriptionItems = pres.PrescriptionDetails.Select(d => new PrescriptionItemDto
                    {
                        MedicineName = d.MedicineName,
                        Dosage = d.Dosage,
                        Quantity = d.Duration
                    }).ToList();
                }

                clinicalTests = await _context.ClinicalTests
                    .Where(t => t.MedicalRecordId == currentRecord.Id)
                    .Select(t => new ClinicManagement.Api.Dtos.ClinicalTests.ClinicalTestDto
                    {
                        Id = t.Id,
                        MedicalRecordId = t.MedicalRecordId,
                        TestName = t.TestName,
                        Result = t.Result,
                        TechnicianName = t.TechnicianName,
                        CreatedAt = t.CreatedAt,
                        Status = string.IsNullOrWhiteSpace(t.Status) ? (string.IsNullOrWhiteSpace(t.Result) ? "Pending" : "Completed") : t.Status,
                        ResultAt = t.ResultAt
                    })
                    .ToListAsync();
            }

            var dto = new ExaminationDetailDto
            {
                AppointmentId = appointment.Id,
                Appointment = new AppointmentDetailDto
                {
                    Id = appointment.Id,
                    AppointmentCode = appointment.AppointmentCode,
                    FullName = appointment.Patient?.FullName,
                    Phone = appointment.Patient?.Phone,
                    Email = appointment.Patient?.Email,
                    DateOfBirth = appointment.Patient?.DateOfBirth ?? DateTime.MinValue,
                    Gender = appointment.Patient?.Gender.ToString(),
                    Address = appointment.Patient?.Address,
                    Reason = appointment.Reason,
                    Status = appointment.Status.ToString(),
                    AppointmentDate = appointment.AppointmentDate,
                    AppointmentTime = appointment.AppointmentTime,
                    CreatedAt = appointment.CreatedAt,
                    PatientCode = appointment.Patient?.PatientCode,
                    CitizenId = appointment.Patient?.CitizenId,
                    InsuranceCardNumber = appointment.Patient?.InsuranceCardNumber
                },
                MedicalHistory = history,
                Diagnosis = currentRecord?.Diagnosis,
                Note = currentRecord?.Note,
                CreatedAt = currentRecord?.CreatedAt ?? appointment.CreatedAt,
                ChiefComplaint = currentRecord?.Symptoms,
                DetailedSymptoms = currentRecord?.DetailedSymptoms,
                PastMedicalHistory = currentRecord?.PastMedicalHistory,
                Allergies = currentRecord?.Allergies,
                Occupation = currentRecord?.Occupation,
                Habits = currentRecord?.Habits,
                HeightCm = currentRecord?.HeightCm,
                WeightKg = currentRecord?.WeightKg,
                Bmi = currentRecord?.Bmi,
                HeartRate = currentRecord?.HeartRate,
                BloodPressure = currentRecord?.BloodPressure,
                Temperature = currentRecord?.Temperature,
                Spo2 = currentRecord?.Spo2,
                InsuranceCoverPercent = currentRecord?.InsuranceCoverPercent ?? 0m,
                Surcharge = currentRecord?.Surcharge ?? 0m,
                Discount = currentRecord?.Discount ?? 0m,
                PrescriptionItems = prescriptionItems,
                ClinicalTests = clinicalTests,
                CurrentMedicalRecordId = currentRecord?.Id
            };

            return Ok(dto);
        }

        [HttpPatch("{id}/complete")]
        public async Task<IActionResult> CompleteAppointment(Guid id)
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.UserId == userId);
            var appointment = await _context.Appointments.FirstOrDefaultAsync(a => a.Id == id && a.DoctorId == doctor.Id);
            if (appointment == null) return NotFound("Appointment not found or not assigned to this doctor");

            appointment.Status = AppointmentStatus.Completed;

            var queue = await _context.QueueEntries
                .FirstOrDefaultAsync(q =>
                    q.AppointmentId == appointment.Id &&
                    (q.Status == QueueStatus.Waiting || q.Status == QueueStatus.InProgress));

            if (queue != null)
            {
                queue.Status = QueueStatus.Done;
                queue.CalledAt ??= DateTime.Now;
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = "Appointment marked as completed" });
        }

        [HttpPost("{id}/transfer-department")]
        public async Task<IActionResult> TransferDepartment(Guid id, [FromBody] TransferDepartmentRequestDto dto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdClaim, out var userId))
                return Unauthorized("Invalid user id in token");

            var doctor = await _context.Doctors
                .Include(d => d.Department)
                .FirstOrDefaultAsync(d => d.UserId == userId);
            if (doctor == null) return Unauthorized("Doctor not found");

            var sourceAppointment = await _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Doctor)
                    .ThenInclude(d => d.Department)
                .FirstOrDefaultAsync(a => a.Id == id && a.DoctorId == doctor.Id);
            if (sourceAppointment == null) return NotFound("Source appointment not found or not assigned to this doctor");

            if (sourceAppointment.Patient == null)
                return BadRequest(new { message = "Source appointment has no patient data." });

            var workDate = dto.AppointmentDate.Date;
            var businessStart = new TimeSpan(7, 0, 0);
            var businessEnd = new TimeSpan(22, 0, 0);
            if (workDate < DateTime.Today)
                return BadRequest(new { message = "Transfer date must be today or later." });
            if (dto.AppointmentTime < businessStart || dto.AppointmentTime > businessEnd)
                return BadRequest(new { message = "Transfer time must be within 07:00 - 22:00." });

            var targetDepartment = await _context.Departments
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.Id == dto.TargetDepartmentId);
            if (targetDepartment == null)
                return BadRequest(new { message = "Target department not found." });

            if (doctor.DepartmentId == dto.TargetDepartmentId)
                return BadRequest(new { message = "Target department must be different from current department." });

            var targetDoctor = await _context.Doctors
                .AsNoTracking()
                .Include(d => d.Department)
                .FirstOrDefaultAsync(d =>
                    d.Id == dto.TargetDoctorId &&
                    d.DepartmentId == dto.TargetDepartmentId &&
                    d.Status != DoctorStatus.Inactive);
            if (targetDoctor == null)
                return BadRequest(new { message = "Target doctor is invalid or inactive." });

            var slotError = await _appointmentBookingService.ValidateDoctorSlotAsync(
                targetDoctor.Id,
                workDate,
                dto.AppointmentTime);
            if (slotError != null)
                return BadRequest(new { message = slotError });

            var hasDuplicate = await _appointmentBookingService.HasExistingAppointmentAsync(
                sourceAppointment.PatientId,
                workDate,
                dto.AppointmentTime);
            if (hasDuplicate)
                return Conflict(new { message = "Patient already has an appointment at this time." });

            await using var transaction = await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable);

            var transferCode = await _appointmentBookingService.GenerateUniqueAppointmentCodeAsync();
            var reasonPrefix = $"[CHUYEN_KHOA:{sourceAppointment.AppointmentCode}->{targetDepartment.Name}]";
            var reasonSuffix = string.IsNullOrWhiteSpace(dto.Reason) ? string.Empty : $" {dto.Reason.Trim()}";

            var transferAppointment = new Appointment
            {
                Id = Guid.NewGuid(),
                PatientId = sourceAppointment.PatientId,
                DoctorId = targetDoctor.Id,
                AppointmentCode = transferCode,
                AppointmentDate = workDate,
                AppointmentTime = dto.AppointmentTime,
                Reason = $"{reasonPrefix}{reasonSuffix}".Trim(),
                Status = AppointmentStatus.Confirmed,
                StaffId = sourceAppointment.StaffId
            };

            _context.Appointments.Add(transferAppointment);

            QueueEntry? createdQueue = null;
            if (dto.EnqueueNow && workDate == DateTime.Today)
            {
                var slot = await _doctorScheduleService.GetEffectiveSlotAsync(targetDoctor.Id, workDate, dto.AppointmentTime);
                if (slot?.RoomId != null)
                {
                    var room = await _context.Rooms
                        .AsNoTracking()
                        .FirstOrDefaultAsync(r => r.Id == slot.RoomId.Value && r.IsActive);

                    if (room != null)
                    {
                        var dayStart = DateTime.Today;
                        var dayEnd = dayStart.AddDays(1);
                        var queueNumber = await _context.QueueEntries
                            .Where(q => q.RoomId == room.Id && q.QueuedAt >= dayStart && q.QueuedAt < dayEnd)
                            .Select(q => (int?)q.QueueNumber)
                            .MaxAsync() ?? 0;

                        createdQueue = new QueueEntry
                        {
                            Id = Guid.NewGuid(),
                            AppointmentId = transferAppointment.Id,
                            RoomId = room.Id,
                            QueueNumber = queueNumber + 1,
                            Status = QueueStatus.Waiting,
                            IsPriority = true,
                            QueuedAt = DateTime.Now
                        };
                        _context.QueueEntries.Add(createdQueue);

                        transferAppointment.Status = AppointmentStatus.CheckedIn;
                        transferAppointment.CheckedInAt = DateTime.UtcNow;
                        transferAppointment.CheckInChannel = "DepartmentTransfer";
                    }
                }
            }

            if (sourceAppointment.Status != AppointmentStatus.Completed)
            {
                sourceAppointment.Status = AppointmentStatus.Completed;
            }

            var sourceQueue = await _context.QueueEntries
                .FirstOrDefaultAsync(q =>
                    q.AppointmentId == sourceAppointment.Id &&
                    (q.Status == QueueStatus.Waiting || q.Status == QueueStatus.InProgress));
            if (sourceQueue != null)
            {
                sourceQueue.Status = QueueStatus.Done;
                sourceQueue.CalledAt ??= DateTime.Now;
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return Ok(new
            {
                message = "Transfer created successfully.",
                sourceAppointmentId = sourceAppointment.Id,
                sourceAppointmentCode = sourceAppointment.AppointmentCode,
                targetAppointmentId = transferAppointment.Id,
                targetAppointmentCode = transferAppointment.AppointmentCode,
                targetDepartment = targetDepartment.Name,
                targetDoctor = targetDoctor.FullName,
                transferAppointmentStatus = transferAppointment.Status.ToString(),
                queueNumber = createdQueue?.QueueNumber
            });
        }
    }
}
