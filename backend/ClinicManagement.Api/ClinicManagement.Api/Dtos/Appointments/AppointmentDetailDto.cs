using System;
using ClinicManagement.Api.Dtos.Appointments;

namespace ClinicManagement.Api.DTOs.Appointments
{
    public class AppointmentDetailDto
    {
        public Guid Id { get; set; }

        public string AppointmentCode { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string? Email { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string Gender { get; set; } = string.Empty;
        public string? Address { get; set; }
        public string? Reason { get; set; }
        public string Status { get; set; } = string.Empty;
        public AppointmentStatusDto StatusDetail { get; set; } = new AppointmentStatusDto();
        public DateTime AppointmentDate { get; set; }
        public TimeSpan AppointmentTime { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
