using ClinicManagement.Api.Models;

namespace ClinicManagement.Api.Dtos.Patients
{
    public class PatientDto
    {
        public Guid Id { get; set; }
        public string FullName { get; set; } = null!;
        public DateTime DateOfBirth { get; set; }
        public Gender Gender { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? Address { get; set; }
        public string? Note { get; set; }
    }
}