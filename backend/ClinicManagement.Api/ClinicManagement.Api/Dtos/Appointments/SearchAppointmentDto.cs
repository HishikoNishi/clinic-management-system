using System;
namespace ClinicManagement.Api.DTOs
{
    public class SearchAppointmentDto
    {
        public string AppointmentCode { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
    }
}

