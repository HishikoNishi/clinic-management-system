using System.ComponentModel.DataAnnotations;

namespace ClinicManagement.Api.Dtos.Doctor
{
    public class UpdateDoctorDto
    {
        [Required]
        [MaxLength(20)]
        public string Code { get; set; } = null!;

        [Required]
        [MaxLength(100)]
        public string Specialty { get; set; } = null!;

        [MaxLength(50)]
        public string? LicenseNumber { get; set; }

        [Required]
        public string Username { get; set; } = null!;
    }
}
