namespace ClinicManagement.Api.Dtos.Doctor
{
    public class DoctorDto
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public string Specialty { get; set; } = null!;
        public Guid DepartmentId { get; set; }
        public string DepartmentName { get; set; } = string.Empty;
        public string? LicenseNumber { get; set; }
        public string Username { get; set; } = null!;
        public string Status { get; set; } = null!;
    }
}
