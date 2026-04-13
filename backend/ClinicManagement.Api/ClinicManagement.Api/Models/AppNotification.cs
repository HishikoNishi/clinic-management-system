using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ClinicManagement.Api.Models
{
    public class AppNotification
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid UserId { get; set; }

        [JsonIgnore]
        public User User { get; set; } = null!;

        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [MaxLength(1000)]
        public string Message { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string Type { get; set; } = "General";

        public bool IsRead { get; set; }

        [MaxLength(500)]
        public string? Link { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
