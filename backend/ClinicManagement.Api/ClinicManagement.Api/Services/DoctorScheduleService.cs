using ClinicManagement.Api.Data;
using ClinicManagement.Api.Dtos.DoctorSchedules;
using ClinicManagement.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Api.Services
{
    public class DoctorScheduleService
    {
        private readonly ClinicDbContext _context;

        public DoctorScheduleService(ClinicDbContext context)
        {
            _context = context;
        }

        public async Task<List<DoctorSchedule>> GetEffectiveSchedulesAsync(Guid doctorId, DateTime workDate)
        {
            var date = workDate.Date;

            var overrideSlots = await _context.DoctorSchedules
                .AsNoTracking()
                .Where(s => s.DoctorId == doctorId && s.WorkDate == date && s.IsActive)
                .OrderBy(s => s.StartTime)
                .ToListAsync();

            var hasOverrideDay = overrideSlots.Count > 0 || await _context.DoctorScheduleOverrideDays
                .AsNoTracking()
                .AnyAsync(o => o.DoctorId == doctorId && o.WorkDate == date);

            if (hasOverrideDay)
            {
                return overrideSlots;
            }

            return await _context.DoctorWeeklySchedules
                .AsNoTracking()
                .Where(s => s.DoctorId == doctorId && s.DayOfWeek == date.DayOfWeek && s.IsActive)
                .OrderBy(s => s.StartTime)
                .Select(s => new DoctorSchedule
                {
                    Id = s.Id,
                    DoctorId = s.DoctorId,
                    WorkDate = date,
                    ShiftCode = s.ShiftCode,
                    SlotLabel = s.SlotLabel,
                    StartTime = s.StartTime,
                    EndTime = s.EndTime,
                    IsActive = s.IsActive,
                    CreatedAt = s.CreatedAt
                })
                .ToListAsync();
        }

        public async Task<DoctorSchedule?> GetEffectiveSlotAsync(Guid doctorId, DateTime workDate, TimeSpan startTime)
        {
            var schedules = await GetEffectiveSchedulesAsync(doctorId, workDate);
            return schedules.FirstOrDefault(s => s.StartTime == startTime && s.IsActive);
        }

        public async Task<bool> HasEffectiveSlotAsync(Guid doctorId, DateTime workDate, TimeSpan startTime)
        {
            var slot = await GetEffectiveSlotAsync(doctorId, workDate, startTime);
            return slot != null;
        }

        public async Task<List<DoctorSchedule>> EnsureOverrideFromEffectiveAsync(Guid doctorId, DateTime workDate)
        {
            var date = workDate.Date;

            var existing = await _context.DoctorSchedules
                .Where(s => s.DoctorId == doctorId && s.WorkDate == date && s.IsActive)
                .OrderBy(s => s.StartTime)
                .ToListAsync();

            var hasOverrideDay = await _context.DoctorScheduleOverrideDays
                .AnyAsync(o => o.DoctorId == doctorId && o.WorkDate == date);

            if (existing.Count > 0 || hasOverrideDay)
            {
                if (!hasOverrideDay)
                {
                    _context.DoctorScheduleOverrideDays.Add(new DoctorScheduleOverrideDay
                    {
                        DoctorId = doctorId,
                        WorkDate = date
                    });
                }

                return existing;
            }

            var effective = await GetEffectiveSchedulesAsync(doctorId, date);

            _context.DoctorScheduleOverrideDays.Add(new DoctorScheduleOverrideDay
            {
                DoctorId = doctorId,
                WorkDate = date
            });

            var created = new List<DoctorSchedule>();
            foreach (var slot in effective)
            {
                var item = new DoctorSchedule
                {
                    DoctorId = doctorId,
                    WorkDate = date,
                    ShiftCode = slot.ShiftCode,
                    SlotLabel = slot.SlotLabel,
                    StartTime = slot.StartTime,
                    EndTime = slot.EndTime,
                    IsActive = true
                };

                created.Add(item);
                _context.DoctorSchedules.Add(item);
            }

            return created;
        }

        public async Task<DoctorWorkSummaryDto> BuildWorkSummaryAsync(Guid doctorId, DateTime referenceDate)
        {
            var today = DateTime.Now.Date;
            var selectedDate = referenceDate.Date;
            var monthStart = new DateTime(today.Year, today.Month, 1);
            var monthEnd = monthStart.AddMonths(1).AddDays(-1);
            var yearStart = new DateTime(today.Year, 1, 1);
            var yearEnd = new DateTime(today.Year, 12, 31);

            var selectedSlots = await CountEffectiveSlotsAsync(doctorId, selectedDate, selectedDate);
            var todaySlots = await CountEffectiveSlotsAsync(doctorId, today, today);
            var currentMonthSlots = await CountEffectiveSlotsAsync(doctorId, monthStart, monthEnd);
            var currentYearSlots = await CountEffectiveSlotsAsync(doctorId, yearStart, yearEnd);

            return new DoctorWorkSummaryDto
            {
                ReferenceDate = selectedDate,
                SelectedDateSlots = selectedSlots,
                SelectedDateMinutes = selectedSlots * 30,
                TodaySlots = todaySlots,
                TodayMinutes = todaySlots * 30,
                CurrentMonthSlots = currentMonthSlots,
                CurrentMonthMinutes = currentMonthSlots * 30,
                CurrentYearSlots = currentYearSlots,
                CurrentYearMinutes = currentYearSlots * 30
            };
        }

        public async Task<int> CountEffectiveSlotsAsync(Guid doctorId, DateTime fromDate, DateTime toDate)
        {
            var start = fromDate.Date;
            var end = toDate.Date;

            var overrideDays = await _context.DoctorScheduleOverrideDays
                .AsNoTracking()
                .Where(o => o.DoctorId == doctorId && o.WorkDate >= start && o.WorkDate <= end)
                .Select(o => o.WorkDate)
                .ToListAsync();

            var overrideSlots = await _context.DoctorSchedules
                .AsNoTracking()
                .Where(s => s.DoctorId == doctorId && s.WorkDate >= start && s.WorkDate <= end && s.IsActive)
                .GroupBy(s => s.WorkDate)
                .Select(g => new { WorkDate = g.Key, Count = g.Count() })
                .ToListAsync();

            var overrideSlotMap = overrideSlots.ToDictionary(x => x.WorkDate.Date, x => x.Count);
            var overrideDates = overrideDays
                .Select(d => d.Date)
                .Concat(overrideSlotMap.Keys)
                .ToHashSet();

            var weeklySlotCounts = await _context.DoctorWeeklySchedules
                .AsNoTracking()
                .Where(s => s.DoctorId == doctorId && s.IsActive)
                .GroupBy(s => s.DayOfWeek)
                .Select(g => new { DayOfWeek = g.Key, Count = g.Count() })
                .ToListAsync();

            var weeklyCountMap = weeklySlotCounts.ToDictionary(x => x.DayOfWeek, x => x.Count);

            var total = 0;
            for (var day = start; day <= end; day = day.AddDays(1))
            {
                if (overrideDates.Contains(day))
                {
                    total += overrideSlotMap.TryGetValue(day, out var count) ? count : 0;
                }
                else
                {
                    total += weeklyCountMap.TryGetValue(day.DayOfWeek, out var count) ? count : 0;
                }
            }

            return total;
        }
    }
}
