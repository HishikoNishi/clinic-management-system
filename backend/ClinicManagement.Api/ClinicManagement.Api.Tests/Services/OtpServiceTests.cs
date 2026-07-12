using ClinicManagement.Api.Models;
using ClinicManagement.Api.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace ClinicManagement.Api.Tests.Services;

public class OtpServiceTests
{
    [Fact]
    public async Task VerifyOtpAsync_ValidOtp_MarksItUsedAndAllowsBooking()
    {
        await using var context = TestDbContextFactory.Create();
        context.EmailOtps.Add(new EmailOtp
        {
            Email = "patient@example.com",
            Code = "123456",
            ExpiredAt = DateTime.UtcNow.AddMinutes(5)
        });
        await context.SaveChangesAsync();
        var sut = new OtpService(context, null!, NullLogger<OtpService>.Instance);

        var verified = await sut.VerifyOtpAsync("patient@example.com", "123456");

        Assert.True(verified);
        Assert.True(await sut.IsVerifiedAsync("patient@example.com"));
        Assert.True((await context.EmailOtps.SingleAsync()).IsUsed);
    }

    [Fact]
    public async Task VerifyOtpAsync_UsedOrExpiredOtp_ReturnsFalse()
    {
        await using var context = TestDbContextFactory.Create();
        context.EmailOtps.AddRange(
            new EmailOtp { Email = "used@example.com", Code = "111111", IsUsed = true, ExpiredAt = DateTime.UtcNow.AddMinutes(5) },
            new EmailOtp { Email = "expired@example.com", Code = "222222", ExpiredAt = DateTime.UtcNow.AddMinutes(-1) });
        await context.SaveChangesAsync();
        var sut = new OtpService(context, null!, NullLogger<OtpService>.Instance);

        Assert.False(await sut.VerifyOtpAsync("used@example.com", "111111"));
        Assert.False(await sut.VerifyOtpAsync("expired@example.com", "222222"));
    }

    [Fact]
    public async Task VerifyOtpAsync_CannotReuseTheSameOtp()
    {
        await using var context = TestDbContextFactory.Create();
        context.EmailOtps.Add(new EmailOtp
        {
            Email = "patient@example.com",
            Code = "654321",
            ExpiredAt = DateTime.UtcNow.AddMinutes(5)
        });
        await context.SaveChangesAsync();
        var sut = new OtpService(context, null!, NullLogger<OtpService>.Instance);

        Assert.True(await sut.VerifyOtpAsync("patient@example.com", "654321"));
        Assert.False(await sut.VerifyOtpAsync("patient@example.com", "654321"));
    }
}
