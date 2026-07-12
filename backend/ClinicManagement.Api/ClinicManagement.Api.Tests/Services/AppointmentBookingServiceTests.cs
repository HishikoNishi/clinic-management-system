using ClinicManagement.Api.DTOs.Appointments;
using ClinicManagement.Api.Models;
using ClinicManagement.Api.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace ClinicManagement.Api.Tests.Services;

public class AppointmentBookingServiceTests
{
    private static AppointmentBookingService CreateSut(ClinicManagement.Api.Data.ClinicDbContext context)
        => new(context, new DoctorScheduleService(context), null!, null!, NullLogger<AppointmentBookingService>.Instance);

    [Fact]
    public void NormalizeInput_TrimsUserSuppliedFields()
    {
        using var context = TestDbContextFactory.Create();
        var sut = CreateSut(context);
        var dto = new CreateAppointmentDto
        {
            FullName = "  Nguyễn An  ", Phone = " 0901000000 ", Email = " patient@example.com ",
            CitizenId = " 012345678901 ", InsuranceCardNumber = " BH123 ", Address = "Đà Nẵng",
            DateOfBirth = new DateTime(1995, 1, 1), Gender = Gender.Male
        };

        sut.NormalizeInput(dto);

        Assert.Equal("Nguyễn An", dto.FullName);
        Assert.Equal("0901000000", dto.Phone);
        Assert.Equal("patient@example.com", dto.Email);
        Assert.Equal("012345678901", dto.CitizenId);
        Assert.Equal("BH123", dto.InsuranceCardNumber);
    }

    [Fact]
    public void MergePatientProfile_PreservesExistingIdentityValuesAndBlankOptionalValues()
    {
        using var context = TestDbContextFactory.Create();
        var sut = CreateSut(context);
        var patient = new Patient
        {
            FullName = "Nguyễn An", Phone = "0901000000", DateOfBirth = new DateTime(1995, 1, 1), Gender = Gender.Male,
            CitizenId = "OLD-CITIZEN", InsuranceCardNumber = "OLD-INSURANCE", Address = "Đà Nẵng", Email = "old@example.com"
        };
        var dto = new CreateAppointmentDto
        {
            FullName = patient.FullName, Phone = patient.Phone!, DateOfBirth = new DateTime(1996, 2, 2), Gender = Gender.Female,
            CitizenId = "NEW-CITIZEN", InsuranceCardNumber = "NEW-INSURANCE", Address = " ", Email = "new@example.com"
        };

        sut.MergePatientProfile(patient, dto);

        Assert.Equal("OLD-CITIZEN", patient.CitizenId);
        Assert.Equal("OLD-INSURANCE", patient.InsuranceCardNumber);
        Assert.Equal("Đà Nẵng", patient.Address);
        Assert.Equal("new@example.com", patient.Email);
        Assert.Equal(dto.DateOfBirth, patient.DateOfBirth);
        Assert.Equal(Gender.Female, patient.Gender);
        Assert.NotNull(patient.UpdatedAt);
    }

    [Fact]
    public async Task ValidateDoctorSlotAsync_ReturnsConflictForAnOccupiedScheduledSlot()
    {
        await using var context = TestDbContextFactory.Create();
        var doctorId = Guid.NewGuid();
        var date = new DateTime(2026, 7, 13);
        var time = new TimeSpan(9, 0, 0);
        context.DoctorWeeklySchedules.Add(new DoctorWeeklySchedule
        {
            DoctorId = doctorId, DayOfWeek = date.DayOfWeek, StartTime = time, EndTime = time.Add(TimeSpan.FromMinutes(30)),
            ShiftCode = "MORNING", SlotLabel = "09:00"
        });
        context.Appointments.Add(new Appointment
        {
            DoctorId = doctorId, PatientId = Guid.NewGuid(), AppointmentDate = date, AppointmentTime = time,
            AppointmentCode = "TEST01", Status = AppointmentStatus.Pending
        });
        await context.SaveChangesAsync();
        var sut = CreateSut(context);

        var message = await sut.ValidateDoctorSlotAsync(doctorId, date, time);

        Assert.Equal("Doctor is already booked at this slot", message);
    }

    [Fact]
    public async Task ValidateDoctorSlotAsync_ReturnsScheduleErrorWhenTheSlotDoesNotExist()
    {
        await using var context = TestDbContextFactory.Create();
        var sut = CreateSut(context);

        var message = await sut.ValidateDoctorSlotAsync(Guid.NewGuid(), new DateTime(2026, 7, 13), new TimeSpan(9, 0, 0));

        Assert.Equal("Doctor does not have a working slot at this time", message);
    }
}
