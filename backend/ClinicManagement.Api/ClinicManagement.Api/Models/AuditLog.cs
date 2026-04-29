namespace ClinicManagement.Api.Models
{
    public class AuditLog
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Action { get; set; } = string.Empty; // Create / Update / SoftDelete / Delete
        public string EntityName { get; set; } = string.Empty;
        public string RecordId { get; set; } = string.Empty;
        public string? UserId { get; set; }
        public string? Username { get; set; }
        public string? BeforeData { get; set; }
        public string? AfterData { get; set; }
        public DateTime ChangedAt { get; set; } = DateTime.UtcNow;
    }
}

