using System;

namespace ClinicManagement.Api.Models
{
    public class Staff
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public string Code { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;

        // GIỮ LẠI Position (theo leader)
        public string? Position { get; set; }

        // SCRUM-161
        public string Role { get; set; } = "Staff";

        // SCRUM-163
        public bool IsActive { get; set; } = true;

        public Guid UserId { get; set; }
        public User User { get; set; } = null!;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}