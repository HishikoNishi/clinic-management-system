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
    public Guid SpecialtyId { get; set; }

    [MaxLength(50)]
    public string? LicenseNumber { get; set; }

    [Required]
    public Guid DepartmentId { get; set; }

    public string? AvatarUrl { get; set; }
}
