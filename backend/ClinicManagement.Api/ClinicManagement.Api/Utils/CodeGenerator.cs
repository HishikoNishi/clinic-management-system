using System;
using System.Linq;

namespace ClinicManagement.Api.Utils
{
    public static class CodeGenerator
    {
        private static readonly Random _random = new();

        public static string GeneratePatientCode()
        {
            const string letters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

            var prefix = new string(Enumerable
                .Repeat(letters, 2)
                .Select(s => s[_random.Next(s.Length)])
                .ToArray());

            var suffix = _random.Next(1000, 10000).ToString();

            return prefix + suffix;
        }

        public static string GenerateAppointmentCode()
        {
            const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";

            return new string(Enumerable
                .Repeat(chars, 6)
                .Select(s => s[_random.Next(s.Length)])
                .ToArray());
        }
    }
}