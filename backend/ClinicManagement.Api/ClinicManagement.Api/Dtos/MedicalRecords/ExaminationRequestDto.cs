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

        public string? Notes { get; set; }

        public bool RequestClinicalTest { get; set; }

        public string? ClinicalTestName { get; set; }

        public List<PrescriptionItemDto> PrescriptionItems { get; set; } = new();
    }
}
