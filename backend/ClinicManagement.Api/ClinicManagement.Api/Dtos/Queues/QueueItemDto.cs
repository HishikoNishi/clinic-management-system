namespace ClinicManagement.Api.Dtos.Queues
{
    public class QueueItemDto
    {
        public Guid Id { get; set; }
        public Guid AppointmentId { get; set; }
        public string AppointmentCode { get; set; } = string.Empty;
        public string? PatientCode { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public Guid? DoctorId { get; set; }
        public string? DoctorName { get; set; }
        public Guid RoomId { get; set; }
        public string RoomCode { get; set; } = string.Empty;
        public string RoomName { get; set; } = string.Empty;
        public string DepartmentName { get; set; } = string.Empty;
        public int QueueNumber { get; set; }
        public string Status { get; set; } = string.Empty;
        public bool IsPriority { get; set; }
        public DateTime QueuedAt { get; set; }
        public DateTime? CalledAt { get; set; }
        public DateTime AppointmentDate { get; set; }
        public TimeSpan AppointmentTime { get; set; }
    }
}
