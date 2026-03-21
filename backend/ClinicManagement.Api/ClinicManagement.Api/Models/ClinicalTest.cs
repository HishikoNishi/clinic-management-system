namespace ClinicManagement.Api.Models
{
    public class ClinicalTest
    {
        public int Id { get; set; }
        public int MedicalRecorId { get; set; }
        public string? Result { get; set; }
        public string? TechnicianName { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public int MedicalRecordId { get; set; }
        public string TestName { get; set; }

    }
}
