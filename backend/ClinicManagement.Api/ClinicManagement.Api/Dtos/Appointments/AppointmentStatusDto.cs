using System;
namespace ClinicManagement.Api.Dtos.Appointments
{
    public class AppointmentStatusDto
    {
        public string Value { get; set; } = string.Empty;
        public string? DoctorName { get; set; }
        public string? DoctorCode { get; set; }
    }

}

