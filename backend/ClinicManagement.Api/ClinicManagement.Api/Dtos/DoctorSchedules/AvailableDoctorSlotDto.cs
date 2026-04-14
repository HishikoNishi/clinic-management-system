namespace ClinicManagement.Api.Dtos.DoctorSchedules
{
    public class AvailableDoctorSlotDto
    {
        public Guid DoctorId { get; set; }
        public string DoctorName { get; set; } = string.Empty;
        public string DoctorCode { get; set; } = string.Empty;
        public Guid DepartmentId { get; set; }
        public string DepartmentName { get; set; } = string.Empty;
        public Guid SpecialtyId { get; set; }
        public string SpecialtyName { get; set; } = string.Empty;
    }
}
