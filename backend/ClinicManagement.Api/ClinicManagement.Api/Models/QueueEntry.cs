using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ClinicManagement.Api.Models
{
    public class QueueEntry
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid AppointmentId { get; set; }

        [JsonIgnore]
        public Appointment? Appointment { get; set; }

        [Required]
        public Guid RoomId { get; set; }

        [JsonIgnore]
        public Room? Room { get; set; }

        [Required]
        public int QueueNumber { get; set; }

        [Required]
        public QueueStatus Status { get; set; } = QueueStatus.Waiting;

        [Required]
        public bool IsPriority { get; set; }

        [Required]
        public DateTime QueuedAt { get; set; } = DateTime.Now;

        public DateTime? CalledAt { get; set; }
    }
}
