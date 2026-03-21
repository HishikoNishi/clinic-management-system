using System.ComponentModel.DataAnnotations;

namespace ClinicManagement.Api.Models
{
    public class Prescription
    {
        [Key]
        public Guid Id { get; set; }

        public Guid MedicalRecordId { get; set; }

        public string? Note { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // navigation tới MedicalRecord
        public MedicalRecord MedicalRecord { get; set; }

        // navigation tới PrescriptionDetails
        public List<PrescriptionDetail> PrescriptionDetails { get; set; } = new();
    }
}