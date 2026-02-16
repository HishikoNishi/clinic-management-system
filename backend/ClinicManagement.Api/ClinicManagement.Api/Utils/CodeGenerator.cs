using System;
namespace ClinicManagement.Api.Utils
{
    public static class CodeGenerator
    {
        public static string GenerateAppointmentCode()
        {
            const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
            var random = new Random();

            return new string(Enumerable
                .Repeat(chars, 6)
                .Select(s => s[random.Next(s.Length)])
                .ToArray());
        }
    }
}

