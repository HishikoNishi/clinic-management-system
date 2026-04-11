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

        // Theo dõi check-in
        public DateTime? CheckedInAt { get; set; }
        public string? CheckInChannel { get; set; }

        public ICollection<Payment> Payments { get; set; } = new List<Payment>();
        [JsonIgnore]
        public ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();

        public DateTime GetAppointmentDateTime()
        {
            return AppointmentDate.Date.Add(AppointmentTime);
        }
        public bool IsNoShow(DateTime now)
        {
            return (Status == AppointmentStatus.Pending || Status == AppointmentStatus.Confirmed)
                   && CheckedInAt == null
                   && GetAppointmentDateTime().AddMinutes(30) < now;
        }
    }
}