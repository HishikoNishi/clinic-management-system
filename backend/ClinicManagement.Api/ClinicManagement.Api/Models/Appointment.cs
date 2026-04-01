using ClinicManagement.Api.Models;
using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ClinicManagement.Api.Models
{
    public class Appointment
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        // =======================
        // Patient
        // =======================
        [Required]
        public Guid PatientId { get; set; }

        [JsonIgnore]
        public Patient? Patient { get; set; }

        // =======================
        // Doctor (staff assign)
        // =======================
        public Guid? DoctorId { get; set; }

        [JsonIgnore]
        public Doctor? Doctor { get; set; }

        // =======================
        // Staff (người xử lý)
        // =======================
        public Guid? StaffId { get; set; }

        [JsonIgnore]
        public Staff? Staff { get; set; }

        // =======================
        // Appointment Info
        // =======================
        [Required]
        public DateTime AppointmentDate { get; set; }

        [Required]
        public TimeSpan AppointmentTime { get; set; }

        [Required]
        [MaxLength(10)]
        public string AppointmentCode { get; set; }
            = $"AP{DateTime.UtcNow:MMddHHmm}";

        public string? Reason { get; set; }

        public AppointmentStatus Status { get; set; }
            = AppointmentStatus.Pending;

        public DateTime CreatedAt { get; set; }
            = DateTime.UtcNow;
    }
}