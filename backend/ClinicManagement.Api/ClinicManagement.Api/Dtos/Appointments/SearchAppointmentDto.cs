using System;
namespace ClinicManagement.Api.DTOs
{
    public class SearchAppointmentDto
    {
        public string? AppointmentCode { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? CitizenId { get; set; }
        public string? InsuranceCardNumber { get; set; }
    }
}
