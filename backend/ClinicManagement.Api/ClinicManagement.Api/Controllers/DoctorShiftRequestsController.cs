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
    [Route("api/doctor/shift-requests")]
    [Authorize(Roles = "Doctor")]
    public class DoctorShiftRequestsController : ControllerBase
    {
        private readonly ClinicDbContext _context;
        private readonly DoctorScheduleService _scheduleService;

        public DoctorShiftRequestsController(ClinicDbContext context, DoctorScheduleService scheduleService)
        {
            _context = context;
            _scheduleService = scheduleService;
        }

        [HttpGet("my")]
        public async Task<IActionResult> GetMyRequests([FromQuery] DoctorShiftRequestStatus? status = null)
        {
            var doctor = await GetCurrentDoctorAsync();
            if (doctor == null)
            {
                return Unauthorized(new { message = "Doctor not found." });
            }

            var query = _context.DoctorShiftRequests
                .AsNoTracking()
                .Include(r => r.Doctor)
                .Include(r => r.PreferredDoctor)
                .Include(r => r.ReplacementDoctor)
                .Where(r => r.DoctorId == doctor.Id);

            if (status.HasValue)
            {
                query = query.Where(r => r.Status == status.Value);
            }

            var requests = await query
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            var result = new List<DoctorShiftRequestDto>();
            foreach (var request in requests)
            {
                result.Add(await MapShiftRequestAsync(request, includeAppointments: false, includeAvailableDoctors: false));
            }

            return Ok(result);
        }

        [HttpGet("pending-count")]
        public async Task<IActionResult> GetMyPendingCount()
        {
            var doctor = await GetCurrentDoctorAsync();
            if (doctor == null)
            {
                return Unauthorized(new { message = "Doctor not found." });
            }

            var count = await _context.DoctorShiftRequests
                .AsNoTracking()
                .CountAsync(r => r.DoctorId == doctor.Id && r.Status == DoctorShiftRequestStatus.Pending);

            return Ok(new { count });
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateDoctorShiftRequestDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var doctor = await GetCurrentDoctorAsync();
            if (doctor == null)
            {
                return Unauthorized(new { message = "Doctor not found." });
            }

            var workDate = dto.WorkDate.Date;
            var now = DateTime.Now;

            var schedule = await _scheduleService.GetEffectiveSlotAsync(doctor.Id, workDate, dto.StartTime);

            if (schedule == null)
            {
                return NotFound(new { message = "Schedule slot not found." });
            }

            if (workDate < now.Date || (workDate == now.Date && schedule.StartTime <= now.TimeOfDay))
            {
                return BadRequest(new { message = "Cannot create a request for a slot that has already started." });
            }

            var existsPending = await _context.DoctorShiftRequests.AnyAsync(r =>
                r.DoctorId == doctor.Id &&
                r.WorkDate == workDate &&
                r.StartTime == dto.StartTime &&
                r.Status == DoctorShiftRequestStatus.Pending);

            if (existsPending)
            {
                return Conflict(new { message = "There is already a pending request for this slot." });
            }

            Doctor? preferredDoctor = null;
            if (dto.PreferredDoctorId.HasValue)
            {
                if (dto.PreferredDoctorId.Value == doctor.Id)
                {
                    return BadRequest(new { message = "Preferred doctor must be different from the current doctor." });
                }

                preferredDoctor = await _context.Doctors
                    .AsNoTracking()
                    .FirstOrDefaultAsync(d => d.Id == dto.PreferredDoctorId.Value && d.Status == DoctorStatus.Active);

                if (preferredDoctor == null)
                {
                    return BadRequest(new { message = "Preferred replacement doctor not found." });
                }
            }

            var request = new DoctorShiftRequest
            {
                DoctorId = doctor.Id,
                RequestType = dto.RequestType,
                WorkDate = workDate,
                ShiftCode = schedule.ShiftCode,
                SlotLabel = schedule.SlotLabel,
                StartTime = schedule.StartTime,
                EndTime = schedule.EndTime,
                Reason = dto.Reason.Trim(),
                PreferredDoctorId = dto.PreferredDoctorId,
                Status = DoctorShiftRequestStatus.Pending
            };

            _context.DoctorShiftRequests.Add(request);
            await _context.SaveChangesAsync();

            return Ok(await MapShiftRequestAsync(request, includeAppointments: false, includeAvailableDoctors: false));
        }

        [HttpGet("/api/doctor/notifications")]
        public async Task<IActionResult> GetNotifications([FromQuery] int take = 10)
        {
            var doctor = await GetCurrentDoctorAsync();
            if (doctor == null)
            {
                return Unauthorized(new { message = "Doctor not found." });
            }

            var safeTake = Math.Clamp(take, 1, 30);

            var notifications = await _context.Notifications
                .AsNoTracking()
                .Where(n => n.UserId == doctor.UserId)
                .OrderByDescending(n => n.CreatedAt)
                .Take(safeTake)
                .Select(n => new AppNotificationDto
                {
                    Id = n.Id,
                    Title = n.Title,
                    Message = n.Message,
                    Type = n.Type,
                    IsRead = n.IsRead,
                    Link = n.Link,
                    CreatedAt = n.CreatedAt
                })
                .ToListAsync();

            var unreadCount = await _context.Notifications
                .AsNoTracking()
                .CountAsync(n => n.UserId == doctor.UserId && !n.IsRead);

            return Ok(new
            {
                unreadCount,
                items = notifications
            });
        }

        [HttpPost("/api/doctor/notifications/{id:guid}/read")]
        public async Task<IActionResult> MarkNotificationAsRead(Guid id)
        {
            var doctor = await GetCurrentDoctorAsync();
            if (doctor == null)
            {
                return Unauthorized(new { message = "Doctor not found." });
            }

            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.Id == id && n.UserId == doctor.UserId);

            if (notification == null)
            {
                return NotFound(new { message = "Notification not found." });
            }

            notification.IsRead = true;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Notification marked as read." });
        }

        private async Task<Doctor?> GetCurrentDoctorAsync()
        {
            var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdValue, out var userId))
            {
                return null;
            }

            return await _context.Doctors
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.UserId == userId);
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
    }
}
