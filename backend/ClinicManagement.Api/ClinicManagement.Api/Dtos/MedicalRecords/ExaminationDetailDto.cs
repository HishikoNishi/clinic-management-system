using ClinicManagement.Api.Dtos.Appointments;
using ClinicManagement.Api.DTOs.Appointments;
using System;
using System.Collections.Generic;

namespace ClinicManagement.Api.Dtos.MedicalRecords
{
    public class ExaminationDetailDto
    {
        public Guid AppointmentId { get; set; }
        public AppointmentDetailDto? Appointment { get; set; }
        public List<BasicMedicalHistoryDto> MedicalHistory { get; set; } = new();
        public string? ChiefComplaint { get; set; }
        public string? DetailedSymptoms { get; set; }
        public string? PastMedicalHistory { get; set; }
        public string? Allergies { get; set; }
        public string? Occupation { get; set; }
        public string? Habits { get; set; }
        public decimal? HeightCm { get; set; }
        public decimal? WeightKg { get; set; }
        public decimal? Bmi { get; set; }
        public int? HeartRate { get; set; }
        public string? BloodPressure { get; set; }
        public decimal? Temperature { get; set; }
        public int? Spo2 { get; set; }
        public string? Diagnosis { get; set; }
        public string? Note { get; set; }
        public decimal InsuranceCoverPercent { get; set; }
        public decimal Surcharge { get; set; }
        public decimal Discount { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<PrescriptionItemDto> PrescriptionItems { get; set; } = new();
        public List<ClinicalTests.ClinicalTestDto> ClinicalTests { get; set; } = new();
        public Guid? CurrentMedicalRecordId { get; set; }
    }
}
