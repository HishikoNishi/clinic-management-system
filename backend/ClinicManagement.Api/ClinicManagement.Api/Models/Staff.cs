using System;

namespace ClinicManagement.Api.Models
{
    public class Staff
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public string Code { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string? Position { get; set; }

        public string Role { get; set; } = "Staff";
        public string? AvatarUrl { get; set; }
        public bool IsActive { get; set; } = true;
        public Guid? DepartmentId { get; set; }
        public Department? Department { get; set; }
        public Guid UserId { get; set; }
        public User User { get; set; } = null!;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}