using System;
using ClinicManagement.Api.Dtos.Doctor;

namespace ClinicManagement.Api.Dtos.Department
{

    public class DepartmentDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<DoctorDto> Doctors { get; set; }
    }
}

