namespace ClinicManagement.Api.DTOs
{
    public class MedicalRecordResponseDto
    {
        public Guid Id { get; set; }
        public Guid PatientId { get; set; }
        public Guid DoctorId { get; set; }

        public string Symptoms { get; set; } = string.Empty;
        public string Diagnosis { get; set; } = string.Empty;
        public string Treatment { get; set; } = string.Empty;
        public string Note { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }
    }
}