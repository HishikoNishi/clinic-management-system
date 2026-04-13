namespace ClinicManagement.Api.Dtos.DoctorSchedules
{
    public class DoctorScheduleSlotImpactDto
    {
        public Guid DoctorId { get; set; }
        public string DoctorName { get; set; } = string.Empty;
        public DateTime WorkDate { get; set; }
        public string ShiftCode { get; set; } = string.Empty;
        public string SlotLabel { get; set; } = string.Empty;
        public string StartTime { get; set; } = string.Empty;
        public string EndTime { get; set; } = string.Empty;
        public int AppointmentCount { get; set; }
        public bool HasAppointments => AppointmentCount > 0;
        public List<DoctorScheduleImpactAppointmentDto> Appointments { get; set; } = new();
    }
}
