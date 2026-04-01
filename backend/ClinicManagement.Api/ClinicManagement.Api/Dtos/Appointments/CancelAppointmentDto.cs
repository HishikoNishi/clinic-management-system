using System;
namespace ClinicManagement.Api.Dtos.Appointments
{
    public class CancelAppointmentDto
    {
        public string FullName { get; set; } = null!;
        public string Phone { get; set; } = null!;
        public string AppointmentCode { get; set; } = null!;
    }

}

