using ClinicManagement.Api.Models;

namespace ClinicManagement.Api.Dtos.ShiftRequests
{
    public class DoctorShiftRequestDto
    {
        public Guid Id { get; set; }
        public Guid DoctorId { get; set; }
        public string DoctorName { get; set; } = string.Empty;
        public DoctorShiftRequestType RequestType { get; set; }
        public DoctorShiftRequestStatus Status { get; set; }
        public DateTime WorkDate { get; set; }
        public string ShiftCode { get; set; } = string.Empty;
        public string SlotLabel { get; set; } = string.Empty;
        public string StartTime { get; set; } = string.Empty;
        public string EndTime { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
        public Guid? PreferredDoctorId { get; set; }
        public string? PreferredDoctorName { get; set; }
        public Guid? ReplacementDoctorId { get; set; }
        public string? ReplacementDoctorName { get; set; }
        public string? AdminNote { get; set; }
        public DateTime? ReviewedAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public int AppointmentCount { get; set; }
        public List<ShiftRequestAppointmentDto> Appointments { get; set; } = new();
        public List<ShiftRequestAvailableDoctorDto> AvailableDoctors { get; set; } = new();
    }
}
