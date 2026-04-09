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
        public string Status { get; set; } = "Pending";
        public DateTime? ResultAt { get; set; }
        public Guid? AppointmentId { get; set; }
        public string? AppointmentCode { get; set; }
        public Guid? PatientId { get; set; }
        public string? PatientName { get; set; }
        public string? PatientPhone { get; set; }
    }
}
