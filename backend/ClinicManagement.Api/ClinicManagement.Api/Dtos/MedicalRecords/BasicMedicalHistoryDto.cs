using System;

namespace ClinicManagement.Api.Dtos.MedicalRecords
{
    public class BasicMedicalHistoryDto
    {
        public Guid Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Diagnosis { get; set; } = string.Empty;
        public string? Note { get; set; }
    }
}
