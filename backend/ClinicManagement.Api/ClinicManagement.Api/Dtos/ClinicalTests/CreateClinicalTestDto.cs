using System;

namespace ClinicManagement.Api.Dtos.ClinicalTests
{
    public class CreateClinicalTestDto
    {
        public Guid MedicalRecordId { get; set; }
        public string TestName { get; set; }
        public Guid? OrderedByDoctorId { get; set; }
    }
}
