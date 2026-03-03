using System.ComponentModel.DataAnnotations;

public class UpdateDoctorDto
{
    [Required]
    public string Code { get; set; } = null!;

    [Required]
    public string Specialty { get; set; } = null!;

    public string? LicenseNumber { get; set; }
}