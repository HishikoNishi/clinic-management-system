using System.ComponentModel.DataAnnotations;

namespace ClinicManagement.Api.Models
{
    public class MedicalRecord
    {
        [Key]
        public Guid Id { get; set; }

        public Guid AppointmentId { get; set; }

        public Guid DoctorId { get; set; }

        public Guid PatientId { get; set; }

        public string Symptoms { get; set; } = string.Empty;

        public string Diagnosis { get; set; } = string.Empty;

        public string Treatment { get; set; } = string.Empty;

        public string Note { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public Doctor Doctor { get; set; } = null!;

        public Appointment Appointment { get; set; } = null!;
    }
}