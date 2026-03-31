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
    public Guid SpecialtyId { get; set; } 

    public string? LicenseNumber { get; set; }
    public string? AvatarUrl { get; set; }

    [Required]
    public Guid DepartmentId { get; set; }
}