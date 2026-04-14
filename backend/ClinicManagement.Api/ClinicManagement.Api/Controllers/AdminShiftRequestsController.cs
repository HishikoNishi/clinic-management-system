using System.Security.Claims;
using ClinicManagement.Api.Data;
using ClinicManagement.Api.Dtos.ShiftRequests;
using ClinicManagement.Api.Models;
using ClinicManagement.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Api.Controllers
{
    [ApiController]
    [Route("api/admin/shift-requests")]
    [Authorize(Roles = "Admin")]
    public class AdminShiftRequestsController : ControllerBase
    {
        private readonly ClinicDbContext _context;
        private readonly DoctorScheduleService _scheduleService;

        public AdminShiftRequestsController(ClinicDbContext context, DoctorScheduleService scheduleService)
        {
            _context = context;
            _scheduleService = scheduleService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] DoctorShiftRequestStatus? status = null, [FromQuery] int take = 50)
        {
            var safeTake = Math.Clamp(take, 1, 100);

            var query = _context.DoctorShiftRequests
                .AsNoTracking()
                .Include(r => r.Doctor)
                .Include(r => r.PreferredDoctor)
                .Include(r => r.ReplacementDoctor)
                .AsQueryable();

            if (status.HasValue)
            {
                query = query.Where(r => r.Status == status.Value);
            }

            var requests = await query
                .OrderBy(r => r.Status == DoctorShiftRequestStatus.Pending ? 0 : 1)
                .ThenBy(r => r.WorkDate)
                .ThenBy(r => r.StartTime)
                .Take(safeTake)
                .ToListAsync();

            var result = new List<DoctorShiftRequestDto>();
            foreach (var request in requests)
            {
                result.Add(await MapShiftRequestAsync(request, includeAppointments: false, includeAvailableDoctors: false));
            }

            return Ok(result);
        }

        [HttpGet("pending-count")]
        public async Task<IActionResult> GetPendingCount()
        {
            var count = await _context.DoctorShiftRequests
                .AsNoTracking()
                .CountAsync(r => r.Status == DoctorShiftRequestStatus.Pending);

            return Ok(new { count });
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetDetail(Guid id)
        {
            var request = await _context.DoctorShiftRequests
                .AsNoTracking()
                .Include(r => r.Doctor)
                .Include(r => r.PreferredDoctor)
                .Include(r => r.ReplacementDoctor)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (request == null)
            {
                return NotFound(new { message = "Shift request not found." });
            }

            return Ok(await MapShiftRequestAsync(request, includeAppointments: true, includeAvailableDoctors: request.Status == DoctorShiftRequestStatus.Pending));
        }

        [HttpPost("{id:guid}/approve")]
        public async Task<IActionResult> Approve(Guid id, [FromBody] ApproveDoctorShiftRequestDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var request = await _context.DoctorShiftRequests
                .Include(r => r.Doctor)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (request == null)
            {
                return NotFound(new { message = "Shift request not found." });
            }

            if (request.Status != DoctorShiftRequestStatus.Pending)
            {
                return BadRequest(new { message = "Only pending requests can be approved." });
            }

            if (request.DoctorId == dto.ReplacementDoctorId)
            {
                return BadRequest(new { message = "Replacement doctor must be different from the requesting doctor." });
            }

            var sourceSlot = await _scheduleService.GetEffectiveSlotAsync(request.DoctorId, request.WorkDate, request.StartTime);

            if (sourceSlot == null)
            {
                return BadRequest(new { message = "The original slot no longer exists." });
            }

            var replacementDoctor = await _context.Doctors
                .Include(d => d.Department)
                .Include(d => d.Specialty)
                .FirstOrDefaultAsync(d => d.Id == dto.ReplacementDoctorId && d.Status == DoctorStatus.Active);

            if (replacementDoctor == null)
            {
                return BadRequest(new { message = "Replacement doctor is not available." });
            }

            if (replacementDoctor.DepartmentId != request.Doctor.DepartmentId)
            {
                return BadRequest(new { message = "Replacement doctor must belong to the same department." });
            }

            var targetHasSchedule = await _scheduleService.HasEffectiveSlotAsync(dto.ReplacementDoctorId, request.WorkDate, request.StartTime);

            if (targetHasSchedule)
            {
                return BadRequest(new { message = "Replacement doctor already has a schedule at this time." });
            }

            var targetHasAppointment = await _context.Appointments.AnyAsync(a =>
                a.DoctorId == dto.ReplacementDoctorId &&
                a.AppointmentDate == request.WorkDate &&
                a.AppointmentTime == request.StartTime &&
                a.Status != AppointmentStatus.Cancelled &&
                a.Status != AppointmentStatus.NoShow);

            if (targetHasAppointment)
            {
                return BadRequest(new { message = "Replacement doctor already has an appointment at this time." });
            }

            var impactedAppointments = await _context.Appointments
                .Where(a =>
                    a.DoctorId == request.DoctorId &&
                    a.AppointmentDate == request.WorkDate &&
                    a.AppointmentTime == request.StartTime &&
                    a.Status != AppointmentStatus.Cancelled &&
                    a.Status != AppointmentStatus.NoShow)
                .ToListAsync();

            var reviewerUserId = GetCurrentUserId();
            if (!reviewerUserId.HasValue)
            {
                return Unauthorized(new { message = "Invalid reviewer." });
            }

            await using var transaction = await _context.Database.BeginTransactionAsync();

            var sourceOverrideSlots = await _scheduleService.EnsureOverrideFromEffectiveAsync(request.DoctorId, request.WorkDate);
            var targetOverrideSlots = await _scheduleService.EnsureOverrideFromEffectiveAsync(replacementDoctor.Id, request.WorkDate);

            var sourceOverrideSlot = sourceOverrideSlots.FirstOrDefault(s => s.StartTime == request.StartTime);
            if (sourceOverrideSlot == null)
            {
                return BadRequest(new { message = "The original slot could not be converted to an override." });
            }

            if (targetOverrideSlots.Any(s => s.StartTime == request.StartTime))
            {
                return BadRequest(new { message = "Replacement doctor already has a schedule at this time." });
            }

            sourceOverrideSlot.DoctorId = replacementDoctor.Id;

            foreach (var appointment in impactedAppointments)
            {
                appointment.DoctorId = replacementDoctor.Id;
                if (appointment.Status == AppointmentStatus.Pending)
                {
                    appointment.Status = AppointmentStatus.Confirmed;
                }
            }

            request.Status = DoctorShiftRequestStatus.Approved;
            request.ReplacementDoctorId = replacementDoctor.Id;
            request.AdminNote = string.IsNullOrWhiteSpace(dto.AdminNote) ? null : dto.AdminNote.Trim();
            request.ReviewedAt = DateTime.UtcNow;
            request.ReviewedByUserId = reviewerUserId.Value;

            var duplicatePendingRequests = await _context.DoctorShiftRequests
                .Where(r =>
                    r.Id != request.Id &&
                    r.DoctorId == request.DoctorId &&
                    r.WorkDate == request.WorkDate &&
                    r.StartTime == request.StartTime &&
                    r.Status == DoctorShiftRequestStatus.Pending)
                .ToListAsync();

            foreach (var other in duplicatePendingRequests)
            {
                other.Status = DoctorShiftRequestStatus.Rejected;
                other.AdminNote = "Slot đã được xử lý bởi một yêu cầu khác.";
                other.ReviewedAt = DateTime.UtcNow;
                other.ReviewedByUserId = reviewerUserId.Value;
            }

            await CreateNotificationAsync(
                request.Doctor.UserId,
                "Yêu cầu đổi ca được chấp thuận",
                $"Yêu cầu cho slot {request.SlotLabel} ngày {request.WorkDate:dd/MM/yyyy} đã được chấp thuận. Bác sĩ thay thế: {replacementDoctor.FullName}.",
                "/doctor/shift-requests");

            foreach (var other in duplicatePendingRequests)
            {
                await CreateNotificationAsync(
                    request.Doctor.UserId,
                    "Yêu cầu ca làm đã đóng",
                    $"Yêu cầu cho slot {other.SlotLabel} ngày {other.WorkDate:dd/MM/yyyy} đã được đóng vì slot này đã được xử lý một yêu cầu khác.",
                    "/doctor/shift-requests");
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return Ok(new
            {
                message = "Shift request approved successfully.",
                movedAppointments = impactedAppointments.Count
            });
        }

        [HttpPost("{id:guid}/reject")]
        public async Task<IActionResult> Reject(Guid id, [FromBody] RejectDoctorShiftRequestDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var request = await _context.DoctorShiftRequests
                .Include(r => r.Doctor)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (request == null)
            {
                return NotFound(new { message = "Shift request not found." });
            }

            if (request.Status != DoctorShiftRequestStatus.Pending)
            {
                return BadRequest(new { message = "Only pending requests can be rejected." });
            }

            var reviewerUserId = GetCurrentUserId();
            if (!reviewerUserId.HasValue)
            {
                return Unauthorized(new { message = "Invalid reviewer." });
            }

            request.Status = DoctorShiftRequestStatus.Rejected;
            request.AdminNote = dto.AdminNote.Trim();
            request.ReviewedAt = DateTime.UtcNow;
            request.ReviewedByUserId = reviewerUserId.Value;

            await CreateNotificationAsync(
                request.Doctor.UserId,
                "Yêu cầu đổi ca bị từ chối",
                $"Yêu cầu cho slot {request.SlotLabel} ngày {request.WorkDate:dd/MM/yyyy} bị từ chối. Lý do: {request.AdminNote}",
                "/doctor/shift-requests");

            await _context.SaveChangesAsync();

            return Ok(new { message = "Shift request rejected successfully." });
        }

        private Guid? GetCurrentUserId()
        {
            var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(userIdValue, out var userId) ? userId : null;
        }

        private async Task<DoctorShiftRequestDto> MapShiftRequestAsync(
            DoctorShiftRequest request,
            bool includeAppointments,
            bool includeAvailableDoctors)
        {
            var sourceSlot = await _scheduleService.GetEffectiveSlotAsync(request.DoctorId, request.WorkDate, request.StartTime);

            var dto = new DoctorShiftRequestDto
            {
                Id = request.Id,
                DoctorId = request.DoctorId,
                DoctorName = request.Doctor?.FullName ?? string.Empty,
                RequestType = request.RequestType,
                Status = request.Status,
                WorkDate = request.WorkDate,
                ShiftCode = request.ShiftCode,
                SlotLabel = request.SlotLabel,
                StartTime = request.StartTime.ToString(@"hh\:mm"),
                EndTime = request.EndTime.ToString(@"hh\:mm"),
                RoomId = sourceSlot?.RoomId,
                RoomCode = sourceSlot?.Room?.Code,
                RoomName = sourceSlot?.Room?.Name,
                Reason = request.Reason,
                PreferredDoctorId = request.PreferredDoctorId,
                PreferredDoctorName = request.PreferredDoctor?.FullName,
                ReplacementDoctorId = request.ReplacementDoctorId,
                ReplacementDoctorName = request.ReplacementDoctor?.FullName,
                AdminNote = request.AdminNote,
                ReviewedAt = request.ReviewedAt,
                CreatedAt = request.CreatedAt
            };

            var appointmentsQuery = _context.Appointments
                .AsNoTracking()
                .Include(a => a.Patient)
                .Where(a =>
                    a.DoctorId == request.DoctorId &&
                    a.AppointmentDate == request.WorkDate &&
                    a.AppointmentTime == request.StartTime &&
                    a.Status != AppointmentStatus.Cancelled &&
                    a.Status != AppointmentStatus.NoShow);

            dto.AppointmentCount = await appointmentsQuery.CountAsync();

            if (includeAppointments)
            {
                dto.Appointments = await appointmentsQuery
                    .OrderBy(a => a.CreatedAt)
                    .Select(a => new ShiftRequestAppointmentDto
                    {
                        AppointmentId = a.Id,
                        AppointmentCode = a.AppointmentCode,
                        PatientName = a.Patient != null ? a.Patient.FullName : string.Empty,
                        Phone = a.Patient != null ? a.Patient.Phone : null,
                        Status = a.Status.ToString()
                    })
                    .ToListAsync();
            }

            if (includeAvailableDoctors)
            {
                dto.AvailableDoctors = await GetAvailableDoctorsAsync(request.DoctorId, request.WorkDate, request.StartTime);
            }

            return dto;
        }

        private async Task<List<ShiftRequestAvailableDoctorDto>> GetAvailableDoctorsAsync(Guid fromDoctorId, DateTime workDate, TimeSpan startTime)
        {
            var sourceDoctor = await _context.Doctors
                .AsNoTracking()
                .Include(d => d.Department)
                .Include(d => d.Specialty)
                .FirstOrDefaultAsync(d => d.Id == fromDoctorId);

            if (sourceDoctor == null)
            {
                return new List<ShiftRequestAvailableDoctorDto>();
            }

            var doctors = await _context.Doctors
                .AsNoTracking()
                .Include(d => d.Department)
                .Include(d => d.Specialty)
                .Where(d =>
                    d.Id != fromDoctorId &&
                    d.Status == DoctorStatus.Active &&
                    d.DepartmentId == sourceDoctor.DepartmentId)
                .OrderBy(d => d.FullName)
                .ToListAsync();

            var busyAppointmentDoctorIds = await _context.Appointments
                .AsNoTracking()
                .Where(a =>
                    a.DoctorId.HasValue &&
                    a.AppointmentDate == workDate &&
                    a.AppointmentTime == startTime &&
                    a.Status != AppointmentStatus.Cancelled &&
                    a.Status != AppointmentStatus.NoShow)
                .Select(a => a.DoctorId!.Value)
                .Distinct()
                .ToListAsync();

            var result = new List<ShiftRequestAvailableDoctorDto>();
            foreach (var doctor in doctors)
            {
                if (busyAppointmentDoctorIds.Contains(doctor.Id))
                {
                    continue;
                }

                var hasEffectiveSlot = await _scheduleService.HasEffectiveSlotAsync(doctor.Id, workDate, startTime);
                if (hasEffectiveSlot)
                {
                    continue;
                }

                result.Add(new ShiftRequestAvailableDoctorDto
                {
                    DoctorId = doctor.Id,
                    DoctorName = doctor.FullName,
                    DoctorCode = doctor.Code,
                    SpecialtyName = doctor.Specialty?.Name ?? string.Empty,
                    DepartmentName = doctor.Department?.Name ?? string.Empty
                });
            }

            return result;
        }

        private Task CreateNotificationAsync(Guid userId, string title, string message, string link)
        {
            _context.Notifications.Add(new AppNotification
            {
                UserId = userId,
                Title = title,
                Message = message,
                Type = "ShiftRequest",
                Link = link
            });

            return Task.CompletedTask;
        }
    }
}
