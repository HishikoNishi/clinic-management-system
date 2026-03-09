namespace ClinicManagement.Api.DTOs
{
    public class CreateMedicalRecordDto
    {
        public Guid AppointmentId { get; set; }

        public string Symptoms { get; set; } = string.Empty;

        public string Diagnosis { get; set; } = string.Empty;

        public string Treatment { get; set; } = string.Empty;

        public string Note { get; set; } = string.Empty;
    }
}