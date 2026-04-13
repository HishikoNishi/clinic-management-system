using System.ComponentModel.DataAnnotations;

namespace ClinicManagement.Api.Dtos.DoctorSchedules
{
    public class SaveDoctorScheduleDayDto
    {
        [Required]
        public DateTime WorkDate { get; set; }

        public List<DoctorScheduleSlotInputDto> Slots { get; set; } = new();
    }
}
