using System.ComponentModel.DataAnnotations;

namespace ClinicManagement.Api.Models
{
    public class MedicalRecord
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid AppointmentId { get; set; }

        public Guid DoctorId { get; set; }

        public Guid PatientId { get; set; }

        public string Symptoms { get; set; }

        public string Diagnosis { get; set; }

        public string Treatment { get; set; }

        public string Note { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}