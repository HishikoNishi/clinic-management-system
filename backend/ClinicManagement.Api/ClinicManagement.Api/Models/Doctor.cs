using System;

namespace ClinicManagement.Api.Models
{
    public class Doctor
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public string Code { get; set; } = string.Empty;       
        public string Specialty { get; set; } = string.Empty;   
        public string LicenseNumber { get; set; } = string.Empty;

        public DoctorStatus Status { get; set; } = DoctorStatus.Active;

        // FK → User
        public Guid UserId { get; set; }
        public User User { get; set; } = null!;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}

