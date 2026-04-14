using ClinicManagement.Api.Data;
using ClinicManagement.Api.Dtos.DoctorSchedules;
using ClinicManagement.Api.Models;
using ClinicManagement.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DoctorSchedulesController : ControllerBase
    {
        private readonly ClinicDbContext _context;
        private readonly DoctorScheduleService _scheduleService;

        public DoctorSchedulesController(ClinicDbContext context, DoctorScheduleService scheduleService)
        {
            _context = context;
            _scheduleService = scheduleService;
        }

        [HttpGet("doctors/{doctorId:guid}")]
        [Authorize(Roles = "Admin,Staff,Doctor")]
        public async Task<IActionResult> GetByDoctor(Guid doctorId, [FromQuery] DateTime date)
        {
            var schedules = await _scheduleService.GetEffectiveSchedulesAsync(doctorId, date.Date);

            return Ok(schedules
                .OrderBy(s => s.StartTime)
                .Select(MapSlotDto)
                .ToList());
        }

        [HttpGet("doctors/{doctorId:guid}/weekly-template")]
        [Authorize(Roles = "Admin,Staff,Doctor")]
        public async Task<IActionResult> GetWeeklyTemplate(Guid doctorId, [FromQuery] DayOfWeek dayOfWeek)
        {
            var schedules = await _context.DoctorWeeklySchedules
                .AsNoTracking()
                .Include(s => s.Room)
                .Where(s => s.DoctorId == doctorId && s.DayOfWeek == dayOfWeek && s.IsActive)
                .OrderBy(s => s.StartTime)
                .Select(s => new DoctorScheduleSlotDto
                {
                    Id = s.Id,
                    ShiftCode = s.ShiftCode,
                    SlotLabel = s.SlotLabel,
                    StartTime = s.StartTime.ToString(@"hh\:mm"),
                    EndTime = s.EndTime.ToString(@"hh\:mm"),
                    RoomId = s.RoomId,
                    RoomCode = s.Room != null ? s.Room.Code : null,
                    RoomName = s.Room != null ? s.Room.Name : null,
                    IsBooked = false
                })
                .ToListAsync();

            return Ok(schedules);
        }

        [HttpPut("doctors/{doctorId:guid}/weekly-template")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> SaveWeeklyTemplate(Guid doctorId, [FromBody] SaveDoctorWeeklyScheduleDto dto)
        {
            var doctor = await _context.Doctors
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.Id == doctorId);

            if (doctor == null)
            {
                return NotFound(new { message = "Doctor not found." });
            }

            var normalizedSlots = dto.Slots
                .GroupBy(s => s.StartTime)
                .Select(g => g.First())
                .OrderBy(s => s.StartTime)
                .ToList();

            var businessStart = new TimeSpan(7, 0, 0);
            var businessEnd = new TimeSpan(22, 0, 0);

            var roomError = await ValidateRoomsAsync(doctor.DepartmentId, normalizedSlots.Select(s => s.RoomId));
            if (roomError != null)
            {
                return BadRequest(new { message = roomError });
            }

            foreach (var slot in normalizedSlots)
            {
                if (string.IsNullOrWhiteSpace(slot.ShiftCode) || string.IsNullOrWhiteSpace(slot.SlotLabel))
                {
                    return BadRequest(new { message = "ShiftCode and SlotLabel are required." });
                }

                if (slot.StartTime >= slot.EndTime)
                {
                    return BadRequest(new { message = "Invalid slot time range." });
                }

                if (slot.StartTime < businessStart || slot.EndTime > businessEnd)
                {
                    return BadRequest(new { message = "Slots must be within 07:00 - 22:00." });
                }
            }

            var existing = await _context.DoctorWeeklySchedules
                .Where(s => s.DoctorId == doctorId && s.DayOfWeek == dto.DayOfWeek)
                .ToListAsync();

            var incomingStartTimes = normalizedSlots
                .Select(s => s.StartTime)
                .ToHashSet();

            var removedStartTimes = existing
                .Where(s => !incomingStartTimes.Contains(s.StartTime))
                .Select(s => s.StartTime)
                .ToList();

            if (removedStartTimes.Count > 0)
            {
                var candidateAppointments = await _context.Appointments
                    .AsNoTracking()
                    .Where(a =>
                        a.DoctorId == doctorId &&
                        a.AppointmentDate >= DateTime.Today &&
                        removedStartTimes.Contains(a.AppointmentTime) &&
                        a.Status != AppointmentStatus.Cancelled &&
                        a.Status != AppointmentStatus.NoShow)
                    .Select(a => new { a.AppointmentDate, a.AppointmentTime })
                    .ToListAsync();

                var overrideDates = await _context.DoctorScheduleOverrideDays
                    .AsNoTracking()
                    .Where(o => o.DoctorId == doctorId && o.WorkDate >= DateTime.Today)
                    .Select(o => o.WorkDate)
                    .ToListAsync();

                var overrideSlotDates = await _context.DoctorSchedules
                    .AsNoTracking()
                    .Where(s => s.DoctorId == doctorId && s.WorkDate >= DateTime.Today && s.IsActive)
                    .Select(s => s.WorkDate)
                    .Distinct()
                    .ToListAsync();

                var overrideDateSet = overrideDates
                    .Select(d => d.Date)
                    .Concat(overrideSlotDates.Select(d => d.Date))
                    .ToHashSet();

                var impacted = candidateAppointments.Any(a =>
                    a.AppointmentDate.DayOfWeek == dto.DayOfWeek &&
                    !overrideDateSet.Contains(a.AppointmentDate.Date));

                if (impacted)
                {
                    return BadRequest(new
                    {
                        message = "Weekly schedule change would remove future booked slots. Use shift reassignment flow for those dates first."
                    });
                }
            }

            _context.DoctorWeeklySchedules.RemoveRange(existing);

            foreach (var slot in normalizedSlots)
            {
                _context.DoctorWeeklySchedules.Add(new DoctorWeeklySchedule
                {
                    DoctorId = doctorId,
                    RoomId = slot.RoomId,
                    DayOfWeek = dto.DayOfWeek,
                    ShiftCode = slot.ShiftCode.Trim(),
                    SlotLabel = slot.SlotLabel.Trim(),
                    StartTime = slot.StartTime,
                    EndTime = slot.EndTime,
                    IsActive = true
                });
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Doctor weekly schedule saved successfully.",
                slotsSaved = normalizedSlots.Count
            });
        }

        [HttpPut("doctors/{doctorId:guid}/day")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> SaveDay(Guid doctorId, [FromBody] SaveDoctorScheduleDayDto dto)
        {
            var doctor = await _context.Doctors
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.Id == doctorId);

            if (doctor == null)
            {
                return NotFound(new { message = "Doctor not found." });
            }

            var workDate = dto.WorkDate.Date;
            var businessStart = new TimeSpan(7, 0, 0);
            var businessEnd = new TimeSpan(22, 0, 0);

            var normalizedSlots = dto.Slots
                .GroupBy(s => s.StartTime)
                .Select(g => g.First())
                .OrderBy(s => s.StartTime)
                .ToList();

            var roomError = await ValidateRoomsAsync(doctor.DepartmentId, normalizedSlots.Select(s => s.RoomId));
            if (roomError != null)
            {
                return BadRequest(new { message = roomError });
            }

            foreach (var slot in normalizedSlots)
            {
                if (string.IsNullOrWhiteSpace(slot.ShiftCode) || string.IsNullOrWhiteSpace(slot.SlotLabel))
                {
                    return BadRequest(new { message = "ShiftCode and SlotLabel are required." });
                }

                if (slot.StartTime >= slot.EndTime)
                {
                    return BadRequest(new { message = "Invalid slot time range." });
                }

                if (slot.StartTime < businessStart || slot.EndTime > businessEnd)
                {
                    return BadRequest(new { message = "Slots must be within 07:00 - 22:00." });
                }
            }

            // Prevent removing booked slots on that day; use reassignment flow first.
            var bookedTimes = await _context.Appointments
                .AsNoTracking()
                .Where(a =>
                    a.DoctorId == doctorId &&
                    a.AppointmentDate == workDate &&
                    a.Status != AppointmentStatus.Cancelled &&
                    a.Status != AppointmentStatus.NoShow)
                .Select(a => a.AppointmentTime)
                .Distinct()
                .ToListAsync();

            if (bookedTimes.Count > 0)
            {
                var incomingTimes = normalizedSlots.Select(s => s.StartTime).ToHashSet();
                var wouldRemoveBooked = bookedTimes.Any(t => !incomingTimes.Contains(t));
                if (wouldRemoveBooked)
                {
                    return BadRequest(new
                    {
                        message = "Day schedule change would remove booked slots. Use reassignment flow for impacted appointments first."
                    });
                }
            }

            var overrideDay = await _context.DoctorScheduleOverrideDays
                .FirstOrDefaultAsync(o => o.DoctorId == doctorId && o.WorkDate == workDate);

            if (overrideDay == null)
            {
                _context.DoctorScheduleOverrideDays.Add(new DoctorScheduleOverrideDay
                {
                    DoctorId = doctorId,
                    WorkDate = workDate
                });
            }

            var existingOverrideSlots = await _context.DoctorSchedules
                .Where(s => s.DoctorId == doctorId && s.WorkDate == workDate)
                .ToListAsync();

            _context.DoctorSchedules.RemoveRange(existingOverrideSlots);

            foreach (var slot in normalizedSlots)
            {
                _context.DoctorSchedules.Add(new DoctorSchedule
                {
                    DoctorId = doctorId,
                    RoomId = slot.RoomId,
                    WorkDate = workDate,
                    ShiftCode = slot.ShiftCode.Trim(),
                    SlotLabel = slot.SlotLabel.Trim(),
                    StartTime = slot.StartTime,
                    EndTime = slot.EndTime,
                    IsActive = true
                });
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Doctor day schedule saved successfully.",
                slotsSaved = normalizedSlots.Count
            });
        }

        [HttpGet("doctors/{doctorId:guid}/work-summary")]
        [Authorize(Roles = "Admin,Staff,Doctor")]
        public async Task<IActionResult> GetWorkSummary(Guid doctorId, [FromQuery] DateTime? date = null)
        {
            var summary = await _scheduleService.BuildWorkSummaryAsync(doctorId, date ?? DateTime.Today);
            return Ok(summary);
        }

        [HttpGet("doctors/{doctorId:guid}/slot-impact")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> GetSlotImpact(Guid doctorId, [FromQuery] DateTime date, [FromQuery] TimeSpan startTime)
        {
            var workDate = date.Date;
            var schedule = await _scheduleService.GetEffectiveSlotAsync(doctorId, workDate, startTime);

            if (schedule == null)
            {
                return NotFound(new { message = "Schedule slot not found." });
            }

            var doctor = await _context.Doctors
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.Id == doctorId);

            var appointments = await _context.Appointments
                .AsNoTracking()
                .Include(a => a.Patient)
                .Where(a =>
                    a.DoctorId == doctorId &&
                    a.AppointmentDate == workDate &&
                    a.AppointmentTime == startTime &&
                    a.Status != AppointmentStatus.Cancelled &&
                    a.Status != AppointmentStatus.NoShow)
                .OrderBy(a => a.CreatedAt)
                .Select(a => new DoctorScheduleImpactAppointmentDto
                {
                    AppointmentId = a.Id,
                    AppointmentCode = a.AppointmentCode,
                    PatientName = a.Patient != null ? a.Patient.FullName : string.Empty,
                    Phone = a.Patient != null ? a.Patient.Phone : null,
                    Status = a.Status.ToString()
                })
                .ToListAsync();

            return Ok(new DoctorScheduleSlotImpactDto
            {
                DoctorId = doctorId,
                DoctorName = doctor?.FullName ?? string.Empty,
                WorkDate = workDate,
                ShiftCode = schedule.ShiftCode,
                SlotLabel = schedule.SlotLabel,
                StartTime = schedule.StartTime.ToString(@"hh\:mm"),
                EndTime = schedule.EndTime.ToString(@"hh\:mm"),
                RoomId = schedule.RoomId,
                RoomCode = schedule.Room?.Code,
                RoomName = schedule.Room?.Name,
                AppointmentCount = appointments.Count,
                Appointments = appointments
            });
        }

        [HttpGet("available-doctors")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> GetAvailableDoctors(
            [FromQuery] Guid fromDoctorId,
            [FromQuery] DateTime date,
            [FromQuery] TimeSpan startTime)
        {
            var workDate = date.Date;

            var sourceDoctor = await _context.Doctors
                .AsNoTracking()
                .Include(d => d.Department)
                .Include(d => d.Specialty)
                .FirstOrDefaultAsync(d => d.Id == fromDoctorId);

            if (sourceDoctor == null)
            {
                return NotFound(new { message = "Source doctor not found." });
            }

            var sourceSlot = await _scheduleService.GetEffectiveSlotAsync(fromDoctorId, workDate, startTime);
            if (sourceSlot == null)
            {
                return NotFound(new { message = "Schedule slot not found." });
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

            var busyByAppointments = await _context.Appointments
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

            var result = new List<AvailableDoctorSlotDto>();
            foreach (var doctor in doctors)
            {
                if (busyByAppointments.Contains(doctor.Id))
                {
                    continue;
                }

                var hasSlot = await _scheduleService.HasEffectiveSlotAsync(doctor.Id, workDate, startTime);
                if (hasSlot)
                {
                    continue;
                }

                result.Add(new AvailableDoctorSlotDto
                {
                    DoctorId = doctor.Id,
                    DoctorName = doctor.FullName,
                    DoctorCode = doctor.Code,
                    DepartmentId = doctor.DepartmentId,
                    DepartmentName = doctor.Department?.Name ?? string.Empty,
                    SpecialtyId = doctor.SpecialtyId,
                    SpecialtyName = doctor.Specialty?.Name ?? string.Empty
                });
            }

            return Ok(result);
        }

        [HttpPost("reassign-slot")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> ReassignSlot([FromBody] ReassignDoctorScheduleSlotDto dto)
        {
            if (dto.FromDoctorId == dto.ToDoctorId)
            {
                return BadRequest(new { message = "Please choose a different replacement doctor." });
            }

            var workDate = dto.WorkDate.Date;

            var sourceSlot = await _scheduleService.GetEffectiveSlotAsync(dto.FromDoctorId, workDate, dto.StartTime);
            if (sourceSlot == null)
            {
                return NotFound(new { message = "Source slot not found." });
            }

            var targetDoctor = await _context.Doctors
                .AsNoTracking()
                .Include(d => d.Department)
                .FirstOrDefaultAsync(d => d.Id == dto.ToDoctorId && d.Status == DoctorStatus.Active);

            if (targetDoctor == null)
            {
                return BadRequest(new { message = "Replacement doctor is not available." });
            }

            var targetHasSchedule = await _scheduleService.HasEffectiveSlotAsync(dto.ToDoctorId, workDate, dto.StartTime);
            if (targetHasSchedule)
            {
                return BadRequest(new { message = "Replacement doctor already has a schedule at this time." });
            }

            var targetHasAppointment = await _context.Appointments.AnyAsync(a =>
                a.DoctorId == dto.ToDoctorId &&
                a.AppointmentDate == workDate &&
                a.AppointmentTime == dto.StartTime &&
                a.Status != AppointmentStatus.Cancelled &&
                a.Status != AppointmentStatus.NoShow);

            if (targetHasAppointment)
            {
                return BadRequest(new { message = "Replacement doctor already has an appointment at this time." });
            }

            var impactedAppointments = await _context.Appointments
                .Where(a =>
                    a.DoctorId == dto.FromDoctorId &&
                    a.AppointmentDate == workDate &&
                    a.AppointmentTime == dto.StartTime &&
                    a.Status != AppointmentStatus.Cancelled &&
                    a.Status != AppointmentStatus.NoShow)
                .ToListAsync();

            if (impactedAppointments.Count > 0 && !dto.MoveAppointments)
            {
                return BadRequest(new { message = "Appointments exist in this slot. Enable appointment transfer." });
            }

            await using var transaction = await _context.Database.BeginTransactionAsync();

            var sourceOverrideSlots = await _scheduleService.EnsureOverrideFromEffectiveAsync(dto.FromDoctorId, workDate);
            var targetOverrideSlots = await _scheduleService.EnsureOverrideFromEffectiveAsync(dto.ToDoctorId, workDate);

            var sourceOverrideSlot = sourceOverrideSlots.FirstOrDefault(s => s.StartTime == dto.StartTime);
            if (sourceOverrideSlot == null)
            {
                return BadRequest(new { message = "Source override slot could not be prepared." });
            }

            if (targetOverrideSlots.Any(s => s.StartTime == dto.StartTime))
            {
                return BadRequest(new { message = "Replacement doctor already has a schedule at this time." });
            }

            sourceOverrideSlot.DoctorId = dto.ToDoctorId;

            foreach (var appointment in impactedAppointments)
            {
                appointment.DoctorId = dto.ToDoctorId;
                if (appointment.Status == AppointmentStatus.Pending)
                {
                    appointment.Status = AppointmentStatus.Confirmed;
                }
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return Ok(new
            {
                message = "Slot reassigned successfully.",
                movedAppointments = impactedAppointments.Count
            });
        }

        [HttpGet("doctors/{doctorId:guid}/available-slots")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAvailableSlots(Guid doctorId, [FromQuery] DateTime date)
        {
            var workDate = date.Date;
            var localNow = DateTime.Now;

            var bookedTimes = await _context.Appointments
                .AsNoTracking()
                .Where(a =>
                    a.DoctorId == doctorId &&
                    a.AppointmentDate == workDate &&
                    a.Status != AppointmentStatus.Cancelled &&
                    a.Status != AppointmentStatus.NoShow)
                .Select(a => a.AppointmentTime)
                .ToListAsync();

            var schedules = await _scheduleService.GetEffectiveSchedulesAsync(doctorId, workDate);

            if (workDate == localNow.Date)
            {
                schedules = schedules
                    .Where(s => s.StartTime > localNow.TimeOfDay)
                    .ToList();
            }

            return Ok(schedules.Select(s =>
            {
                var dto = MapSlotDto(s);
                dto.IsBooked = bookedTimes.Contains(s.StartTime);
                return dto;
            }).ToList());
        }

        [HttpDelete("{id:guid}")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var schedule = await _context.DoctorSchedules.FindAsync(id);
            if (schedule == null)
            {
                return NotFound(new { message = "Schedule slot not found." });
            }

            var hasAppointments = await _context.Appointments.AnyAsync(a =>
                a.DoctorId == schedule.DoctorId &&
                a.AppointmentDate == schedule.WorkDate &&
                a.AppointmentTime == schedule.StartTime &&
                a.Status != AppointmentStatus.Cancelled &&
                a.Status != AppointmentStatus.NoShow);

            if (hasAppointments)
            {
                return BadRequest(new { message = "This slot already has appointments. Use reassignment flow instead." });
            }

            var hasOverrideDay = await _context.DoctorScheduleOverrideDays
                .AnyAsync(o => o.DoctorId == schedule.DoctorId && o.WorkDate == schedule.WorkDate);

            if (!hasOverrideDay)
            {
                _context.DoctorScheduleOverrideDays.Add(new DoctorScheduleOverrideDay
                {
                    DoctorId = schedule.DoctorId,
                    WorkDate = schedule.WorkDate
                });
            }

            _context.DoctorSchedules.Remove(schedule);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Schedule slot deleted successfully." });
        }

        private static DoctorScheduleSlotDto MapSlotDto(DoctorSchedule schedule)
        {
            return new DoctorScheduleSlotDto
            {
                Id = schedule.Id,
                ShiftCode = schedule.ShiftCode,
                SlotLabel = schedule.SlotLabel,
                StartTime = schedule.StartTime.ToString(@"hh\:mm"),
                EndTime = schedule.EndTime.ToString(@"hh\:mm"),
                RoomId = schedule.RoomId,
                RoomCode = schedule.Room?.Code,
                RoomName = schedule.Room?.Name,
                IsBooked = false
            };
        }

        private async Task<string?> ValidateRoomsAsync(Guid doctorDepartmentId, IEnumerable<Guid> roomIds)
        {
            var roomIdList = roomIds.ToList();
            if (roomIdList.Count == 0 || roomIdList.Any(id => id == Guid.Empty))
            {
                return "Room is required for each schedule slot.";
            }

            var requiredRoomIds = roomIdList
                .Distinct()
                .ToList();

            var rooms = await _context.Rooms
                .AsNoTracking()
                .Where(r => requiredRoomIds.Contains(r.Id) && r.IsActive)
                .Select(r => new { r.Id, r.DepartmentId })
                .ToListAsync();

            if (rooms.Count != requiredRoomIds.Count)
            {
                return "One or more selected rooms are invalid or inactive.";
            }

            if (rooms.Any(r => r.DepartmentId != doctorDepartmentId))
            {
                return "Schedule room must belong to the same department as the doctor.";
            }

            return null;
        }
    }
}
