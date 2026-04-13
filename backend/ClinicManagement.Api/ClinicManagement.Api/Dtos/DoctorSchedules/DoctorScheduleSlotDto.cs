namespace ClinicManagement.Api.Dtos.DoctorSchedules
{
    public class DoctorScheduleSlotDto
    {
        public Guid Id { get; set; }
        public string ShiftCode { get; set; } = string.Empty;
        public string SlotLabel { get; set; } = string.Empty;
        public string StartTime { get; set; } = string.Empty;
        public string EndTime { get; set; } = string.Empty;
        public bool IsBooked { get; set; }
    }
}
