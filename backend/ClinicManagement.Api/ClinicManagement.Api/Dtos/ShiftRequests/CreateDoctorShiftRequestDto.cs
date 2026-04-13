using System.ComponentModel.DataAnnotations;
using ClinicManagement.Api.Models;

namespace ClinicManagement.Api.Dtos.ShiftRequests
{
    public class CreateDoctorShiftRequestDto
    {
        [Required]
        public DoctorShiftRequestType RequestType { get; set; }

        [Required]
        public DateTime WorkDate { get; set; }

        [Required]
        public TimeSpan StartTime { get; set; }

        [Required]
        [MaxLength(1000)]
        public string Reason { get; set; } = string.Empty;

        public Guid? PreferredDoctorId { get; set; }
    }
}
