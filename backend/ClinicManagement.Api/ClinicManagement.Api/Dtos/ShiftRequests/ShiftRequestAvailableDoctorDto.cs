namespace ClinicManagement.Api.Dtos.ShiftRequests
{
    public class ShiftRequestAvailableDoctorDto
    {
        public Guid DoctorId { get; set; }
        public string DoctorName { get; set; } = string.Empty;
        public string DoctorCode { get; set; } = string.Empty;
        public string SpecialtyName { get; set; } = string.Empty;
        public string DepartmentName { get; set; } = string.Empty;
    }
}
