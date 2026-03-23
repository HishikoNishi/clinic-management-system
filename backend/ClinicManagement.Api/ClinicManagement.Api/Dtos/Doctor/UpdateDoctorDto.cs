using System;
using System.ComponentModel.DataAnnotations;

public class UpdateDoctorDto
{
    [Required]
    public string Code { get; set; } = null!;

        [Required]
        [MaxLength(30)]
        public string FullName { get; set; } = null!;

        [Required]
        [MaxLength(100)]
        public string Specialty { get; set; } = null!;

        [MaxLength(50)]
        public string? LicenseNumber { get; set; }
    public Guid DepartmentId { get; set; }
}

