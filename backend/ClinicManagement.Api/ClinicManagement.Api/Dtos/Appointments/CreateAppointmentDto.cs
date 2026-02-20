using System;
using System.ComponentModel.DataAnnotations;
using ClinicManagement.Api.Models;

namespace ClinicManagement.Api.DTOs.Appointments
{
    public class CreateAppointmentDto
    {
        [Required]
        public string FullName { get; set; } = null!;

        [Required]
        public string Phone { get; set; } = null!;

        [Required]
        public DateTime DateOfBirth { get; set; }

        [Required]
        public Gender Gender { get; set; }

        public string? Email { get; set; }
        public string Address { get; set; }

        [Required]
        public DateTime AppointmentDate { get; set; }

        [Required]
        public TimeSpan AppointmentTime { get; set; }

        public string? Reason { get; set; }
    }
}
