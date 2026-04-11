using System;

namespace ClinicManagement.Api.Dtos.ClinicalTests
{
    public class PatientTestSummaryDto
    {
        public Guid PatientId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string? AppointmentCode { get; set; }
        public int PendingCount { get; set; }
        public Guid AppointmentId { get; set; }
        public Guid MedicalRecordId { get; set; }
        public string? PatientCode { get; set; }
        public string? CitizenId { get; set; }
        public string? InsuranceCardNumber { get; set; }
    }
}
