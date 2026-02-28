using System.ComponentModel.DataAnnotations;

namespace ClinicManagement.Api.Dtos.Doctor
{
    public class CreateDoctorDto
    {
        [Required]
        [MaxLength(100)]
        public string Username { get; set; } = null!;

        [Required]
        [MinLength(6)]
        public string Password { get; set; } = null!;

        [Required]
        [MaxLength(20)]
        public string Code { get; set; } = null!;

        [Required]
        [MaxLength(100)]
        public string Specialty { get; set; } = null!;

        [MaxLength(50)]
        public string? LicenseNumber { get; set; }
    }
}
