using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ClinicManagement.Api.Models
{
    public class Appointment
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid PatientId { get; set; }

        public Guid? DoctorId { get; set; } 

        [Required]
        public DateTime AppointmentDate { get; set; }

        [Required]
        public TimeSpan AppointmentTime { get; set; }
        [Required]
        [MaxLength(10)]
        public string AppointmentCode { get; set; } = null!;


        public string? Reason { get; set; }

        public AppointmentStatus Status { get; set; }
            = AppointmentStatus.Pending;

        public DateTime CreatedAt { get; set; }
            = DateTime.UtcNow;

        [JsonIgnore]
        public Patient? Patient { get; set; }

        [JsonIgnore]
        public Doctor? Doctor { get; set; }

    }
}
