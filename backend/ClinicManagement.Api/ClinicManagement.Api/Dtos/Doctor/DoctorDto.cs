    namespace ClinicManagement.Api.Dtos.Doctor
{
    public class DoctorDto
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public string Specialty { get; set; } = null!;
        public string? LicenseNumber { get; set; }
        public string Username { get; set; } = null!;
        public string Status { get; set; } = null!;
    }
}
