using System.ComponentModel.DataAnnotations;

namespace ClinicManagement.Api.Dtos.ShiftRequests
{
    public class ApproveDoctorShiftRequestDto
    {
        [Required]
        public Guid ReplacementDoctorId { get; set; }

        [MaxLength(1000)]
        public string? AdminNote { get; set; }
    }
}
