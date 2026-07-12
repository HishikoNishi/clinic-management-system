using ClinicManagement.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Api.Tests;

internal static class TestDbContextFactory
{
    public static ClinicDbContext Create()
    {
        var options = new DbContextOptionsBuilder<ClinicDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;

        return new ClinicDbContext(options);
    }
}
