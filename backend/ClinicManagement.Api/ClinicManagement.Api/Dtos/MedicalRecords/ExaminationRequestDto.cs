using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ClinicManagement.Api.Dtos.MedicalRecords
{
    public class ExaminationRequestDto
    {
        [Required]
        public Guid AppointmentId { get; set; }

        [Required]
        public string Diagnosis { get; set; } = string.Empty;

        public string? ChiefComplaint { get; set; }
        public string? DetailedSymptoms { get; set; }
        public string? PastMedicalHistory { get; set; }
        public string? Allergies { get; set; }
        public string? Occupation { get; set; }
        public string? Habits { get; set; }

        public decimal? HeightCm { get; set; }
        public decimal? WeightKg { get; set; }
        public string? BloodPressure { get; set; }
        public int? HeartRate { get; set; }
        public decimal? Temperature { get; set; }
        public int? Spo2 { get; set; }

        public string? Notes { get; set; }

        public bool RequestClinicalTest { get; set; }

        // Cho phép nhiều xét nghiệm; giữ trường cũ làm fallback tương thích
        public List<string>? ClinicalTestNames { get; set; }
        public string? ClinicalTestName { get; set; }

        public List<PrescriptionItemDto> PrescriptionItems { get; set; } = new();

        // Thông tin thanh toán/bảo hiểm
        public decimal InsuranceCoverPercent { get; set; } = 0m; // 0-1
        public decimal Surcharge { get; set; } = 0m;
        public decimal Discount { get; set; } = 0m;
    }
}
