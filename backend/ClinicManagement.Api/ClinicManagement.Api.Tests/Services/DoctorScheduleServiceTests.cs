using ClinicManagement.Api.Models;
using ClinicManagement.Api.Services;
using Xunit;

namespace ClinicManagement.Api.Tests.Services;

public class DoctorScheduleServiceTests
{
    [Fact]
    public async Task GetEffectiveSchedulesAsync_UsesWeeklyTemplateWhenNoOverrideExists()
    {
        await using var context = TestDbContextFactory.Create();
        var doctorId = Guid.NewGuid();
        var date = new DateTime(2026, 7, 13, 15, 30, 0);
        context.DoctorWeeklySchedules.Add(new DoctorWeeklySchedule
        {
            DoctorId = doctorId,
            DayOfWeek = date.DayOfWeek,
            StartTime = new TimeSpan(8, 0, 0),
            EndTime = new TimeSpan(8, 30, 0),
            ShiftCode = "MORNING",
            SlotLabel = "08:00"
        });
        await context.SaveChangesAsync();
        var sut = new DoctorScheduleService(context);

        var schedules = await sut.GetEffectiveSchedulesAsync(doctorId, date);

        var slot = Assert.Single(schedules);
        Assert.Equal(date.Date, slot.WorkDate);
        Assert.Equal(new TimeSpan(8, 0, 0), slot.StartTime);
    }

    [Fact]
    public async Task GetEffectiveSchedulesAsync_OverrideDayReplacesWeeklyTemplate()
    {
        await using var context = TestDbContextFactory.Create();
        var doctorId = Guid.NewGuid();
        var date = new DateTime(2026, 7, 13);
        context.DoctorWeeklySchedules.Add(new DoctorWeeklySchedule
        {
            DoctorId = doctorId, DayOfWeek = date.DayOfWeek,
            StartTime = new TimeSpan(8, 0, 0), EndTime = new TimeSpan(8, 30, 0),
            ShiftCode = "MORNING", SlotLabel = "08:00"
        });
        context.DoctorScheduleOverrideDays.Add(new DoctorScheduleOverrideDay { DoctorId = doctorId, WorkDate = date });
        context.DoctorSchedules.Add(new DoctorSchedule
        {
            DoctorId = doctorId, WorkDate = date,
            StartTime = new TimeSpan(10, 0, 0), EndTime = new TimeSpan(10, 30, 0),
            ShiftCode = "OVERRIDE", SlotLabel = "10:00"
        });
        await context.SaveChangesAsync();
        var sut = new DoctorScheduleService(context);

        var schedules = await sut.GetEffectiveSchedulesAsync(doctorId, date);

        var slot = Assert.Single(schedules);
        Assert.Equal("OVERRIDE", slot.ShiftCode);
        Assert.Equal(new TimeSpan(10, 0, 0), slot.StartTime);
    }

    [Fact]
    public async Task CountEffectiveSlotsAsync_UsesZeroSlotsForAnOverrideDayWithoutSlots()
    {
        await using var context = TestDbContextFactory.Create();
        var doctorId = Guid.NewGuid();
        var monday = new DateTime(2026, 7, 13);
        var tuesday = monday.AddDays(1);
        context.DoctorWeeklySchedules.AddRange(
            new DoctorWeeklySchedule { DoctorId = doctorId, DayOfWeek = monday.DayOfWeek, StartTime = new TimeSpan(8, 0, 0), EndTime = new TimeSpan(8, 30, 0), ShiftCode = "M", SlotLabel = "08:00" },
            new DoctorWeeklySchedule { DoctorId = doctorId, DayOfWeek = monday.DayOfWeek, StartTime = new TimeSpan(8, 30, 0), EndTime = new TimeSpan(9, 0, 0), ShiftCode = "M", SlotLabel = "08:30" },
            new DoctorWeeklySchedule { DoctorId = doctorId, DayOfWeek = tuesday.DayOfWeek, StartTime = new TimeSpan(8, 0, 0), EndTime = new TimeSpan(8, 30, 0), ShiftCode = "M", SlotLabel = "08:00" });
        context.DoctorScheduleOverrideDays.Add(new DoctorScheduleOverrideDay { DoctorId = doctorId, WorkDate = monday });
        await context.SaveChangesAsync();
        var sut = new DoctorScheduleService(context);

        var count = await sut.CountEffectiveSlotsAsync(doctorId, monday, tuesday);

        Assert.Equal(1, count);
    }
}
