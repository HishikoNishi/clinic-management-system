using System;
using System.ComponentModel.DataAnnotations;


namespace ClinicManagement.Api.Models
{
    public class Doctor
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(20)]
        public string Code { get; set; } = string.Empty;

        [Required]
        [MaxLength(30)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        public Guid SpecialtyId { get; set; }
        public Specialty Specialty { get; set; } = null!;


        [MaxLength(50)]
        public string LicenseNumber { get; set; } = string.Empty;

        public DoctorStatus Status { get; set; } = DoctorStatus.Active;
        public Guid DepartmentId { get; set; }
        public Department? Department { get; set; }
        public string? AvatarUrl { get; set; }
        // ? FK ? User 
        public Guid UserId { get; set; }
        public User User { get; set; } = null!;
        public ICollection<Appointment> Appointments { get; set; }
        = new List<Appointment>();
        public ICollection<DoctorSchedule> Schedules { get; set; }
        = new List<DoctorSchedule>();
        public ICollection<DoctorWeeklySchedule> WeeklySchedules { get; set; }
        = new List<DoctorWeeklySchedule>();
        public ICollection<DoctorScheduleOverrideDay> ScheduleOverrideDays { get; set; }
        = new List<DoctorScheduleOverrideDay>();
        public ICollection<DoctorShiftRequest> ShiftRequests { get; set; }
        = new List<DoctorShiftRequest>();
        public ICollection<DoctorShiftRequest> PreferredShiftRequests { get; set; }
        = new List<DoctorShiftRequest>();
        public ICollection<DoctorShiftRequest> ReplacementShiftRequests { get; set; }
        = new List<DoctorShiftRequest>();
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
