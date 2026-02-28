namespace ClinicManagement.Api.Dtos.Staff
{
    public class StaffDto
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public string? Position { get; set; }
        public string Username { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
    }
}
