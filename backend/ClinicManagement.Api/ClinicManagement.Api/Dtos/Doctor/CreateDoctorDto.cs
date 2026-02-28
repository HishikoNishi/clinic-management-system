using System.ComponentModel.DataAnnotations;

namespace ClinicManagement.Api.Dtos.Doctor
{
    public class CreateDoctorDto
    {
        [Required]
        public Guid UserId { get; set; }

        [Required]
        [MaxLength(20)]
        public string Code { get; set; } = null!;
        [Required]
        [MaxLength(30)]
        public string FullName { get; set; } = null!;
        [Required]
        [MaxLength(100)]
        public string Specialty { get; set; } = null!;

        [MaxLength(50)]
        public string? LicenseNumber { get; set; }
    }
}
