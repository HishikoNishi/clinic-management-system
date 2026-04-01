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
    }
}
