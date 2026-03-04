using System.ComponentModel.DataAnnotations;

namespace ClinicManagement.Api.Models
{
    public class Prescription
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid MedicalRecordId { get; set; }

        public string? Note { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public MedicalRecord? MedicalRecord { get; set; }

        public ICollection<PrescriptionDetail>? Details { get; set; }
    }
}