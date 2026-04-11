using System;
using System.Linq;

namespace ClinicManagement.Api.Utils
{
    public static class CodeGenerator
    {
        public static string GeneratePatientCode()
        {
            const string letters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            var random = new Random();

            var prefix = new string(Enumerable
                .Repeat(letters, 2)
                .Select(s => s[random.Next(s.Length)])
                .ToArray());

            var suffix = random.Next(1000, 10000).ToString();
            return prefix + suffix;
        }

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
