namespace ClinicManagement.Api.Dtos.ShiftRequests
{
    public class ShiftRequestAppointmentDto
    {
        public Guid AppointmentId { get; set; }
        public string AppointmentCode { get; set; } = string.Empty;
        public string PatientName { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
