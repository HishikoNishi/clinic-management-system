using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ClinicManagement.Api.Models
{
    public class DoctorShiftRequest
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid DoctorId { get; set; }

        [JsonIgnore]
        public Doctor Doctor { get; set; } = null!;

        [Required]
        public DoctorShiftRequestType RequestType { get; set; } = DoctorShiftRequestType.EmergencyLeave;

        [Required]
        public DateTime WorkDate { get; set; }

        [Required]
        [MaxLength(32)]
        public string ShiftCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(64)]
        public string SlotLabel { get; set; } = string.Empty;

        [Required]
        public TimeSpan StartTime { get; set; }

        [Required]
        public TimeSpan EndTime { get; set; }

        [Required]
        [MaxLength(1000)]
        public string Reason { get; set; } = string.Empty;

        public Guid? PreferredDoctorId { get; set; }

        [JsonIgnore]
        public Doctor? PreferredDoctor { get; set; }

        public Guid? ReplacementDoctorId { get; set; }

        [JsonIgnore]
        public Doctor? ReplacementDoctor { get; set; }

        [Required]
        public DoctorShiftRequestStatus Status { get; set; } = DoctorShiftRequestStatus.Pending;

        [MaxLength(1000)]
        public string? AdminNote { get; set; }

        public Guid? ReviewedByUserId { get; set; }

        [JsonIgnore]
        public User? ReviewedByUser { get; set; }

        public DateTime? ReviewedAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
