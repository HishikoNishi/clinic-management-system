using System;

namespace ClinicManagement.Api.Dtos.ClinicalTests
{
    public class ClinicalTestDto
    {
        public int Id { get; set; }
        public Guid MedicalRecordId { get; set; }
        public string TestName { get; set; } = string.Empty;
        public string? Result { get; set; }
        public string? TechnicianName { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Status => string.IsNullOrWhiteSpace(Result) ? "Pending" : "Completed";
    }
}
