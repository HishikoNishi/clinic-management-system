using System.ComponentModel.DataAnnotations;

namespace ClinicManagement.Api.Dtos.DoctorSchedules
{
    public class DoctorScheduleSlotInputDto
    {
        [Required]
        public string ShiftCode { get; set; } = string.Empty;

        [Required]
        public string SlotLabel { get; set; } = string.Empty;

        [Required]
        public TimeSpan StartTime { get; set; }

        [Required]
        public TimeSpan EndTime { get; set; }
    }
}
