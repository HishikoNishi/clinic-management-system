using System;
using System.ComponentModel.DataAnnotations;

namespace ClinicManagement.Api.Models
{
    public class Doctor
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(20)]
        public string Code { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string Specialty { get; set; } = string.Empty;

        [MaxLength(50)]
        public string LicenseNumber { get; set; } = string.Empty;

        public DoctorStatus Status { get; set; } = DoctorStatus.Active;

        // ✅ FK → User 
        public Guid UserId { get; set; }
        public User User { get; set; } = null!;
        public ICollection<Appointment> Appointments { get; set; }
        = new List<Appointment>();
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;


        public ICollection<Appointment> Appointments { get; set; }
            = new List<Appointment>();
    }
}