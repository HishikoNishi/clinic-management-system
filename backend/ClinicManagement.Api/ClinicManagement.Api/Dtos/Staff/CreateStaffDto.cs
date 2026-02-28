using System.ComponentModel.DataAnnotations;

namespace ClinicManagement.Api.Dtos.Staff
{
    public class CreateStaffDto
    {
        [Required]
        public Guid UserId { get; set; }

        [Required]
        [MaxLength(20)]
        public string Code { get; set; } = null!;

        [Required]
        [MaxLength(100)]
        public string FullName { get; set; } = null!;

        [MaxLength(100)]
        public string? Position { get; set; }
    }
}
