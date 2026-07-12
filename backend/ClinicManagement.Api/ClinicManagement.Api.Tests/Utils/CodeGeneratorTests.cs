using System.Text.RegularExpressions;
using ClinicManagement.Api.Utils;
using Xunit;

namespace ClinicManagement.Api.Tests.Utils;

public class CodeGeneratorTests
{
    [Fact]
    public void GeneratePatientCode_ReturnsTwoLettersFollowedByFourDigits()
    {
        var code = CodeGenerator.GeneratePatientCode();

        Assert.Matches(new Regex("^[A-Z]{2}\\d{4}$"), code);
    }

    [Fact]
    public void GenerateAppointmentCode_UsesSixAllowedCharacters()
    {
        var code = CodeGenerator.GenerateAppointmentCode();

        Assert.Matches(new Regex("^[ABCDEFGHJKLMNPQRSTUVWXYZ23456789]{6}$"), code);
    }
}
