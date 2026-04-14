using System.ComponentModel.DataAnnotations;

namespace ClinicManagement.Api.Dtos.DoctorSchedules
{
    public class SaveDoctorWeeklyScheduleDto
    {
        [Required]
        public DayOfWeek DayOfWeek { get; set; }

        public List<DoctorScheduleSlotInputDto> Slots { get; set; } = new();
    }
}
