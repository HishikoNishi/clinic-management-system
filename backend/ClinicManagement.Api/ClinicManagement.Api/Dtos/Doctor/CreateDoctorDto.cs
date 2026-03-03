using System.ComponentModel.DataAnnotations;

public class CreateDoctorDto
{
    [Required]
    public string Username { get; set; } = null!;

    [Required]
    public string Password { get; set; } = null!;

    [Required]
    public string Code { get; set; } = null!;

    [Required]
    public string Specialty { get; set; } = null!;

    public string? LicenseNumber { get; set; }
}