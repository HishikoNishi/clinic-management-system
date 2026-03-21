namespace ClinicManagement.Api.Models
{
    public class ClinicalTest
    {
        public int Id { get; set; }
        public Guid MedicalRecordId { get; set; }
        public string TestName { get; set; } = string.Empty;
        public string? Result { get; set; }
        public string? TechnicianName { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public MedicalRecord? MedicalRecord { get; set; }
    }
}
