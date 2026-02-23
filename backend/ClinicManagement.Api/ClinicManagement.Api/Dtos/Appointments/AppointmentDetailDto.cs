using System;
namespace ClinicManagement.Api.DTOs.Appointments
{
    public class AppointmentDetailDto
    {
        public string AppointmentCode { get; set; }
        public string FullName { get; set; }
        public string Phone { get; set; }
        public string? Email { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string Gender { get; set; }
        public string? Address { get; set; }
        public string? Reason { get; set; }
        public string Status { get; set; }
        public DateTime AppointmentDate { get; set; }
        public TimeSpan AppointmentTime { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}



