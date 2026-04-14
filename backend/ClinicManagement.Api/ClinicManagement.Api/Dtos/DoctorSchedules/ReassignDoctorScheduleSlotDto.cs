using System.ComponentModel.DataAnnotations;

namespace ClinicManagement.Api.Dtos.DoctorSchedules
{
    public class ReassignDoctorScheduleSlotDto
    {
        [Required]
        public Guid FromDoctorId { get; set; }

        [Required]
        public Guid ToDoctorId { get; set; }

        [Required]
        public DateTime WorkDate { get; set; }

        [Required]
        public TimeSpan StartTime { get; set; }

        public bool MoveAppointments { get; set; } = true;
    }
}
