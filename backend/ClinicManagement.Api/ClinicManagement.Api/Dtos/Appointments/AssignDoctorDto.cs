using System;
namespace ClinicManagement.Api.DTOs.Appointments
{
    public class AssignDoctorDto
    {
        public Guid AppointmentId { get; set; }
        public Guid DoctorId { get; set; }
    }
}


