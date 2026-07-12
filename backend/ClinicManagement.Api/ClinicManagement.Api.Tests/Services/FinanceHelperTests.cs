using ClinicManagement.Api.Services;
using Xunit;

namespace ClinicManagement.Api.Tests.Services;

public class FinanceHelperTests
{
    [Theory]
    [InlineData(-0.1, 0.0)]
    [InlineData(0.0, 0.0)]
    [InlineData(0.75, 0.75)]
    [InlineData(1.0, 1.0)]
    [InlineData(1.5, 1.0)]
    public void Clamp01_KeepsInsuranceCoverageWithinZeroAndOne(decimal input, decimal expected)
    {
        Assert.Equal(expected, FinanceHelper.Clamp01(input));
    }
}
