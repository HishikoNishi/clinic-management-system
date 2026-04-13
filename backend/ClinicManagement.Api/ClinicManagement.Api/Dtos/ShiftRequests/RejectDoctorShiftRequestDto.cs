using System.ComponentModel.DataAnnotations;

namespace ClinicManagement.Api.Dtos.ShiftRequests
{
    public class RejectDoctorShiftRequestDto
    {
        [Required]
        [MaxLength(1000)]
        public string AdminNote { get; set; } = string.Empty;
    }
}
