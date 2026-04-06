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
        public string? Diagnosis { get; set; }
        public string? Note { get; set; }
        public decimal InsuranceCoverPercent { get; set; }
        public decimal Surcharge { get; set; }
        public decimal Discount { get; set; }
        public List<PrescriptionItemDto> PrescriptionItems { get; set; } = new();
        public List<string> ClinicalTests { get; set; } = new();
    }
}
