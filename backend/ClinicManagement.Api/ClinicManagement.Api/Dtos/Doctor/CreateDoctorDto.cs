using System.ComponentModel.DataAnnotations;

public class CreateDoctorDto
{
<<<<<<< HEAD
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
=======
    [Required]
    public string Username { get; set; } = null!;

    [Required]
    public string Password { get; set; } = null!;

    [Required]
    public string Code { get; set; } = null!;

    [Required]
    public string Specialty { get; set; } = null!;
>>>>>>> origin/feature/doctor-frontend-MinhThu

    public string? LicenseNumber { get; set; }
}