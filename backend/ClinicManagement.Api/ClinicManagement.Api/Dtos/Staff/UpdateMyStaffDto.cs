using System.ComponentModel.DataAnnotations;

namespace ClinicManagement.Api.Dtos.Staff
{
    public class UpdateMyStaffDto
    {
        [Required]
        public string Code { get; set; } = null!;

        [Required]
        public string FullName { get; set; } = null!;

        public bool IsActive { get; set; }

        public string? AvatarUrl { get; set; }
    }
}
