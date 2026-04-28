using System.ComponentModel.DataAnnotations;

namespace ClinicManagement.Api.Dtos.Appointments
{
    public class TransferDepartmentRequestDto
    {
        [Required]
        public Guid TargetDepartmentId { get; set; }

        [Required]
        public Guid TargetDoctorId { get; set; }

        [Required]
        public DateTime AppointmentDate { get; set; }

        [Required]
        public TimeSpan AppointmentTime { get; set; }

        public string? Reason { get; set; }

        public bool EnqueueNow { get; set; } = true;
    }
}
