using ClinicManagement.Api.Dtos.Prescription;
using System;
using System.Collections.Generic;

namespace ClinicManagement.Api.DTOs
{
    public class CreatePrescriptionDto
    {
        public Guid MedicalRecordId { get; set; }

        public string Note { get; set; }

        public List<CreatePrescriptionDetailDto> Details { get; set; }
    }
}