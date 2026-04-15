using System.Data;
using System.Security.Claims;
using ClinicManagement.Api.Data;
using ClinicManagement.Api.Dtos.Queues;
using ClinicManagement.Api.Models;
using ClinicManagement.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RoomQueuesController : ControllerBase
    {
        private readonly ClinicDbContext _context;
        private readonly DoctorScheduleService _scheduleService;

        public RoomQueuesController(ClinicDbContext context, DoctorScheduleService scheduleService)
        {
            _context = context;
            _scheduleService = scheduleService;
        }

        [HttpGet("rooms")]
        [Authorize(Roles = "Admin,Staff,Doctor")]
        public async Task<ActionResult<IEnumerable<RoomOptionDto>>> GetRooms([FromQuery] Guid? departmentId = null)
        {
            var query = _context.Rooms
                .AsNoTracking()
                .Include(r => r.Department)
                .Where(r => r.IsActive);

            if (departmentId.HasValue && departmentId.Value != Guid.Empty)
            {
                query = query.Where(r => r.DepartmentId == departmentId.Value);
            }

            var rooms = await query
                .OrderBy(r => r.Department!.Name)
                .ThenBy(r => r.Name)
                .Select(r => new RoomOptionDto
                {
                    Id = r.Id,
                    Code = r.Code,
                    Name = r.Name,
                    DepartmentId = r.DepartmentId,
                    DepartmentName = r.Department != null ? r.Department.Name : string.Empty
                })
                .ToListAsync();

            return Ok(rooms);
        }

        [HttpPost("check-in")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<ActionResult<QueueItemDto>> CheckIn([FromBody] QueueCheckInDto dto)
        {
            if (dto.AppointmentId == Guid.Empty)
            {
                return BadRequest(new { message = "AppointmentId la bat buoc" });
            }

            var appointment = await _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Doctor)
                .FirstOrDefaultAsync(a => a.Id == dto.AppointmentId);

            if (appointment == null)
            {
                return NotFound(new { message = "Khong tim thay lich kham" });
            }

            if (appointment.Status == AppointmentStatus.Cancelled ||
                appointment.Status == AppointmentStatus.Completed ||
                appointment.Status == AppointmentStatus.NoShow)
            {
                return BadRequest(new { message = "Lich kham nay khong the check-in vao hang cho" });
            }

            var assignment = await ResolveCheckInAssignmentAsync(appointment, dto.RoomId);
            if (assignment.Room == null)
            {
                return BadRequest(new
                {
                    message = appointment.DoctorId.HasValue
                        ? "Bac si cua lich kham chua co phong trong slot nay"
                        : "Phong duoc chon chua co bac si truc trong slot nay"
                });
            }

            var room = assignment.Room;

            var dayStart = DateTime.Today;
            var dayEnd = dayStart.AddDays(1);

            var existingQueue = await _context.QueueEntries
                .Include(q => q.Room)
                    .ThenInclude(r => r!.Department)
                .Include(q => q.Appointment)
                    .ThenInclude(a => a!.Patient)
                .Include(q => q.Appointment)
                    .ThenInclude(a => a!.Doctor)
                .FirstOrDefaultAsync(q =>
                    q.AppointmentId == appointment.Id &&
                    q.QueuedAt >= dayStart &&
                    q.QueuedAt < dayEnd &&
                    q.Status != QueueStatus.Skipped);

            if (existingQueue != null)
            {
                return BadRequest(new
                {
                    message = "Benh nhan da co so thu tu trong ngay hom nay",
                    queue = MapQueueItem(existingQueue)
                });
            }

            using var transaction = await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable);

            var queueNumber = await _context.QueueEntries
                .Where(q => q.RoomId == room.Id && q.QueuedAt >= dayStart && q.QueuedAt < dayEnd)
                .Select(q => (int?)q.QueueNumber)
                .MaxAsync() ?? 0;

            var queueEntry = new QueueEntry
            {
                Id = Guid.NewGuid(),
                AppointmentId = appointment.Id,
                RoomId = room.Id,
                QueueNumber = queueNumber + 1,
                Status = QueueStatus.Waiting,
                IsPriority = appointment.StaffId == null,
                QueuedAt = DateTime.Now
            };

            if (!appointment.DoctorId.HasValue && assignment.DoctorId.HasValue)
            {
                appointment.DoctorId = assignment.DoctorId.Value;
            }

            appointment.Status = AppointmentStatus.CheckedIn;
            appointment.CheckedInAt ??= DateTime.UtcNow;
            appointment.CheckInChannel ??= "RoomQueue";

            _context.QueueEntries.Add(queueEntry);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            queueEntry = await _context.QueueEntries
                .Include(q => q.Room)
                    .ThenInclude(r => r!.Department)
                .Include(q => q.Appointment)
                    .ThenInclude(a => a!.Patient)
                .Include(q => q.Appointment)
                    .ThenInclude(a => a!.Doctor)
                .FirstAsync(q => q.Id == queueEntry.Id);

            return Ok(MapQueueItem(queueEntry));
        }

        [HttpGet("rooms/{roomId:guid}")]
        [Authorize(Roles = "Admin,Staff,Doctor")]
        public async Task<ActionResult<RoomQueueStateDto>> GetRoomQueue(Guid roomId)
        {
            Guid? doctorId = null;
            if (User.IsInRole("Doctor"))
            {
                doctorId = await GetCurrentDoctorIdAsync();
                if (!doctorId.HasValue)
                {
                    return Unauthorized(new { message = "Khong xac dinh duoc bac si hien tai" });
                }

                var roomOwnedByDoctor = await DoctorOwnsRoomAsync(doctorId.Value, roomId, DateTime.Today);

                if (!roomOwnedByDoctor)
                {
                    return Forbid();
                }
            }

            var state = await BuildRoomStateAsync(roomId, doctorId);
            if (state == null)
            {
                return NotFound(new { message = "Khong tim thay phong kham" });
            }

            return Ok(state);
        }

        [HttpGet("doctor/rooms")]
        [Authorize(Roles = "Doctor")]
        public async Task<ActionResult<IEnumerable<DoctorRoomQueueSummaryDto>>> GetDoctorRooms()
        {
            var doctorId = await GetCurrentDoctorIdAsync();
            if (!doctorId.HasValue)
            {
                return Unauthorized(new { message = "Khong xac dinh duoc bac si hien tai" });
            }

            var dayStart = DateTime.Today;
            var dayEnd = dayStart.AddDays(1);
            var roomIds = await GetDoctorRoomIdsAsync(doctorId.Value, dayStart);

            if (roomIds.Count == 0)
            {
                return Ok(Array.Empty<DoctorRoomQueueSummaryDto>());
            }

            var rooms = await _context.Rooms
                .AsNoTracking()
                .Include(r => r.Department)
                .Where(r => r.IsActive && roomIds.Contains(r.Id))
                .OrderBy(r => r.Name)
                .Select(r => new DoctorRoomQueueSummaryDto
                {
                    RoomId = r.Id,
                    RoomCode = r.Code,
                    RoomName = r.Name,
                    DepartmentId = r.DepartmentId,
                    DepartmentName = r.Department != null ? r.Department.Name : string.Empty,
                    WaitingCount = _context.QueueEntries.Count(q =>
                        q.RoomId == r.Id &&
                        q.QueuedAt >= dayStart &&
                        q.QueuedAt < dayEnd &&
                        q.Status == QueueStatus.Waiting),
                    InProgressCount = _context.QueueEntries.Count(q =>
                        q.RoomId == r.Id &&
                        q.QueuedAt >= dayStart &&
                        q.QueuedAt < dayEnd &&
                        q.Status == QueueStatus.InProgress),
                    TotalToday = _context.QueueEntries.Count(q =>
                        q.RoomId == r.Id &&
                        q.QueuedAt >= dayStart &&
                        q.QueuedAt < dayEnd)
                })
                .ToListAsync();

            return Ok(rooms);
        }

        [HttpPost("rooms/{roomId:guid}/next")]
        [Authorize(Roles = "Doctor")]
        public async Task<ActionResult<QueueItemDto>> CallNext(Guid roomId)
        {
            var doctorId = await GetCurrentDoctorIdAsync();
            if (!doctorId.HasValue)
            {
                return Unauthorized(new { message = "Khong xac dinh duoc bac si hien tai" });
            }

            var room = await _context.Rooms
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Id == roomId && r.IsActive);
            if (room == null)
            {
                return NotFound(new { message = "Khong tim thay phong kham" });
            }

            if (!await DoctorOwnsRoomAsync(doctorId.Value, roomId, DateTime.Today))
            {
                return Forbid();
            }

            var dayStart = DateTime.Today;
            var dayEnd = dayStart.AddDays(1);

            var current = await _context.QueueEntries
                .Include(q => q.Room)
                    .ThenInclude(r => r!.Department)
                .Include(q => q.Appointment)
                    .ThenInclude(a => a!.Patient)
                .Include(q => q.Appointment)
                    .ThenInclude(a => a!.Doctor)
                .FirstOrDefaultAsync(q =>
                    q.RoomId == roomId &&
                    q.QueuedAt >= dayStart &&
                    q.QueuedAt < dayEnd &&
                    q.Status == QueueStatus.InProgress);

            if (current != null)
            {
                return BadRequest(new
                {
                    message = "Dang co benh nhan trong phong. Hay hoan tat hoac bo luot truoc khi goi tiep.",
                    current = MapQueueItem(current)
                });
            }

            var next = await _context.QueueEntries
                .Include(q => q.Room)
                    .ThenInclude(r => r!.Department)
                .Include(q => q.Appointment)
                    .ThenInclude(a => a!.Patient)
                .Include(q => q.Appointment)
                    .ThenInclude(a => a!.Doctor)
                .Where(q =>
                    q.RoomId == roomId &&
                    q.QueuedAt >= dayStart &&
                    q.QueuedAt < dayEnd &&
                    q.Status == QueueStatus.Waiting)
                .OrderByDescending(q => q.IsPriority)
                .ThenBy(q => q.QueueNumber)
                .FirstOrDefaultAsync();

            if (next == null)
            {
                return NotFound(new { message = "Khong con benh nhan dang cho trong phong nay" });
            }
            // Gan bac si cho lich kham neu benh nhan dat lich khong chon bac si (DoctorId null).
            // Neu lich da gan bac si khac thi chan goi nham phong/nham bac si.
            if (next.Appointment != null)
            {
                if (!next.Appointment.DoctorId.HasValue)
                {
                    next.Appointment.DoctorId = doctorId.Value;
                }
                else if (next.Appointment.DoctorId.Value != doctorId.Value)
                {
                    return BadRequest(new
                    {
                        message = "Lich kham nay da duoc gan cho bac si khac, khong the goi vao phong nay."
                    });
                }
            }
            next.Status = QueueStatus.InProgress;
            next.CalledAt = DateTime.Now;

            var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.Id == doctorId.Value);
            if (doctor != null && doctor.Status != DoctorStatus.Inactive)
            {
                doctor.Status = DoctorStatus.Busy;
            }

            await _context.SaveChangesAsync();

            return Ok(MapQueueItem(next));
        }

        [HttpPost("{queueId:guid}/done")]
        [Authorize(Roles = "Doctor")]
        public async Task<ActionResult<QueueItemDto>> MarkDone(Guid queueId)
        {
            return await UpdateQueueStatusAsync(queueId, QueueStatus.Done);
        }

        [HttpPost("{queueId:guid}/skip")]
        [Authorize(Roles = "Doctor")]
        public async Task<ActionResult<QueueItemDto>> MarkSkipped(Guid queueId)
        {
            return await UpdateQueueStatusAsync(queueId, QueueStatus.Skipped);
        }

        [HttpGet("patient/{appointmentCode}")]
        [AllowAnonymous]
        public async Task<ActionResult<PatientQueueStatusDto>> GetPatientQueueStatus(string appointmentCode)
        {
            if (string.IsNullOrWhiteSpace(appointmentCode))
            {
                return BadRequest(new { message = "AppointmentCode la bat buoc" });
            }

            var appointment = await _context.Appointments
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.AppointmentCode == appointmentCode);

            if (appointment == null)
            {
                return NotFound(new { message = "Khong tim thay lich kham" });
            }

            var dayStart = DateTime.Today;
            var dayEnd = dayStart.AddDays(1);

            var ownQueue = await _context.QueueEntries
                .Include(q => q.Room)
                    .ThenInclude(r => r!.Department)
                .Include(q => q.Appointment)
                    .ThenInclude(a => a!.Patient)
                .Include(q => q.Appointment)
                    .ThenInclude(a => a!.Doctor)
                .Where(q =>
                    q.AppointmentId == appointment.Id &&
                    q.QueuedAt >= dayStart &&
                    q.QueuedAt < dayEnd)
                .OrderByDescending(q => q.QueuedAt)
                .FirstOrDefaultAsync();

            if (ownQueue == null)
            {
                return Ok(new PatientQueueStatusDto
                {
                    AppointmentCode = appointment.AppointmentCode,
                    AppointmentStatus = appointment.Status.ToString(),
                    WaitingAhead = 0,
                    Message = "Benh nhan chua duoc check-in vao hang cho"
                });
            }

            var roomState = await BuildRoomStateAsync(ownQueue.RoomId, null);
            var currentCalling = roomState?.CurrentCalling;

            var waitingAhead = 0;
            if (ownQueue.Status == QueueStatus.Waiting && roomState != null)
            {
                waitingAhead = roomState.Items.Count(item =>
                    item.Status == QueueStatus.Waiting.ToString() &&
                    ((item.IsPriority && !ownQueue.IsPriority) ||
                     (item.IsPriority == ownQueue.IsPriority && item.QueueNumber < ownQueue.QueueNumber)));
            }

            return Ok(new PatientQueueStatusDto
            {
                AppointmentCode = appointment.AppointmentCode,
                AppointmentStatus = appointment.Status.ToString(),
                OwnQueue = MapQueueItem(ownQueue),
                CurrentCalling = currentCalling,
                WaitingAhead = waitingAhead,
                Message = ownQueue.Status == QueueStatus.Done
                    ? "Benh nhan da duoc goi xong trong ngay hom nay"
                    : null
            });
        }

        private async Task<ActionResult<QueueItemDto>> UpdateQueueStatusAsync(Guid queueId, QueueStatus status)
        {
            var doctorId = await GetCurrentDoctorIdAsync();
            if (!doctorId.HasValue)
            {
                return Unauthorized(new { message = "Khong xac dinh duoc bac si hien tai" });
            }

            var queue = await _context.QueueEntries
                .Include(q => q.Room)
                    .ThenInclude(r => r!.Department)
                .Include(q => q.Appointment)
                    .ThenInclude(a => a!.Patient)
                .Include(q => q.Appointment)
                    .ThenInclude(a => a!.Doctor)
                .FirstOrDefaultAsync(q => q.Id == queueId);

            if (queue == null)
            {
                return NotFound(new { message = "Khong tim thay so thu tu" });
            }

            if (!await DoctorOwnsRoomAsync(doctorId.Value, queue.RoomId, DateTime.Today))
            {
                return NotFound(new { message = "Khong tim thay so thu tu" });
            }

            // Chan hoan tat khi bac si chua luu ho so kham (tranh "hoan tat" khi chua kham).
            if (status == QueueStatus.Done)
            {
                if (queue.Status != QueueStatus.InProgress)
                {
                    return BadRequest(new { message = "Chi co the hoan tat khi benh nhan dang duoc kham." });
                }

                var hasMedicalRecord = await _context.MedicalRecords.AnyAsync(m =>
                    m.AppointmentId == queue.AppointmentId &&
                    m.DoctorId == doctorId.Value);

                if (!hasMedicalRecord)
                {
                    return BadRequest(new { message = "Chua co ho so kham. Vui long luu ho so kham truoc khi hoan tat." });
                }
            }

            queue.Status = status;
            if (status == QueueStatus.InProgress)
            {
                queue.CalledAt = DateTime.Now;
            }

            var hasAnotherInProgress = await _context.QueueEntries.AnyAsync(q =>
                q.Id != queue.Id &&
                q.Status == QueueStatus.InProgress &&
                q.Appointment != null &&
                q.Appointment.DoctorId == doctorId.Value);

            if (!hasAnotherInProgress)
            {
                var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.Id == doctorId.Value);
                if (doctor != null && doctor.Status == DoctorStatus.Busy)
                {
                    doctor.Status = DoctorStatus.Active;
                }
            }

            await _context.SaveChangesAsync();

            return Ok(MapQueueItem(queue));
        }

        private async Task<Guid?> GetCurrentDoctorIdAsync()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdClaim, out var userId))
            {
                return null;
            }

            return await _context.Doctors
                .Where(d => d.UserId == userId)
                .Select(d => (Guid?)d.Id)
                .FirstOrDefaultAsync();
        }

        private async Task<RoomQueueStateDto?> BuildRoomStateAsync(Guid roomId, Guid? doctorId)
        {
            var room = await _context.Rooms
                .AsNoTracking()
                .Include(r => r.Department)
                .FirstOrDefaultAsync(r =>
                    r.Id == roomId &&
                    r.IsActive);

            if (room == null)
            {
                return null;
            }

            if (doctorId.HasValue && !await DoctorOwnsRoomAsync(doctorId.Value, roomId, DateTime.Today))
            {
                return null;
            }

            var dayStart = DateTime.Today;
            var dayEnd = dayStart.AddDays(1);

            var query = _context.QueueEntries
                .AsNoTracking()
                .Include(q => q.Room)
                    .ThenInclude(r => r!.Department)
                .Include(q => q.Appointment)
                    .ThenInclude(a => a!.Patient)
                .Include(q => q.Appointment)
                    .ThenInclude(a => a!.Doctor)
                .Where(q =>
                    q.RoomId == roomId &&
                    q.QueuedAt >= dayStart &&
                    q.QueuedAt < dayEnd &&
                    (q.Status == QueueStatus.Waiting || q.Status == QueueStatus.InProgress));

            var queueItems = await query
                .OrderByDescending(q => q.Status == QueueStatus.InProgress)
                .ThenByDescending(q => q.IsPriority)
                .ThenBy(q => q.QueueNumber)
                .ToListAsync();

            return new RoomQueueStateDto
            {
                RoomId = room.Id,
                RoomCode = room.Code,
                RoomName = room.Name,
                DepartmentId = room.DepartmentId,
                DepartmentName = room.Department?.Name ?? string.Empty,
                WaitingCount = queueItems.Count(q => q.Status == QueueStatus.Waiting),
                InProgressCount = queueItems.Count(q => q.Status == QueueStatus.InProgress),
                CurrentCalling = queueItems
                    .Where(q => q.Status == QueueStatus.InProgress)
                    .Select(MapQueueItem)
                    .FirstOrDefault(),
                Items = queueItems.Select(MapQueueItem).ToList()
            };
        }

        private async Task<(Room? Room, Guid? DoctorId)> ResolveCheckInAssignmentAsync(Appointment appointment, Guid? requestedRoomId)
        {
            if (appointment.DoctorId.HasValue)
            {
                var slot = await _scheduleService.GetEffectiveSlotAsync(
                    appointment.DoctorId.Value,
                    appointment.AppointmentDate,
                    appointment.AppointmentTime);

                if (slot?.RoomId != null)
                {
                    var scheduledRoom = await _context.Rooms
                        .Include(r => r.Department)
                        .FirstOrDefaultAsync(r => r.Id == slot.RoomId.Value && r.IsActive);

                    return (scheduledRoom, appointment.DoctorId.Value);
                }
                return (null, null);
            }

            if (!requestedRoomId.HasValue || requestedRoomId.Value == Guid.Empty)
            {
                return (null, null);
            }

            var scheduledSlot = await _scheduleService.GetEffectiveSlotByRoomAsync(
                requestedRoomId.Value,
                appointment.AppointmentDate,
                appointment.AppointmentTime);

            if (scheduledSlot?.RoomId != null)
            {
                var scheduledRoom = await _context.Rooms
                    .Include(r => r.Department)
                    .FirstOrDefaultAsync(r => r.Id == scheduledSlot.RoomId.Value && r.IsActive);

                return (scheduledRoom, scheduledSlot.DoctorId);
            }
            return (null, null);
        }

        private async Task<bool> DoctorOwnsRoomAsync(Guid doctorId, Guid roomId, DateTime workDate)
        {
            var roomIds = await GetDoctorRoomIdsAsync(doctorId, workDate);
            return roomIds.Contains(roomId);
        }

        private async Task<List<Guid>> GetDoctorRoomIdsAsync(Guid doctorId, DateTime workDate)
        {
            var scheduleRoomIds = await _scheduleService.GetDoctorRoomIdsForDateAsync(doctorId, workDate);
            return scheduleRoomIds;
        }

        private static QueueItemDto MapQueueItem(QueueEntry queue)
        {
            return new QueueItemDto
            {
                Id = queue.Id,
                AppointmentId = queue.AppointmentId,
                AppointmentCode = queue.Appointment?.AppointmentCode ?? string.Empty,
                PatientCode = queue.Appointment?.Patient?.PatientCode,
                FullName = queue.Appointment?.Patient?.FullName ?? string.Empty,
                Phone = queue.Appointment?.Patient?.Phone,
                DoctorId = queue.Appointment?.DoctorId,
                DoctorName = queue.Appointment?.Doctor?.FullName,
                RoomId = queue.RoomId,
                RoomCode = queue.Room?.Code ?? string.Empty,
                RoomName = queue.Room?.Name ?? string.Empty,
                DepartmentName = queue.Room?.Department?.Name ?? string.Empty,
                QueueNumber = queue.QueueNumber,
                Status = queue.Status.ToString(),
                IsPriority = queue.IsPriority,
                QueuedAt = queue.QueuedAt,
                CalledAt = queue.CalledAt,
                AppointmentDate = queue.Appointment?.AppointmentDate ?? DateTime.MinValue,
                AppointmentTime = queue.Appointment?.AppointmentTime ?? TimeSpan.Zero
            };
        }
    }
}
