using System;
using System.ComponentModel.DataAnnotations;

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
        public string? LicenseNumber { get; set; }
    public string? AvatarUrl { get; set; }
    public Guid DepartmentId { get; set; }
    public Guid SpecialtyId { get; set; }

}