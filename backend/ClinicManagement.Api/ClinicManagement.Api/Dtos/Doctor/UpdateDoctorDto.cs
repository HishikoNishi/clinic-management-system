using System.ComponentModel.DataAnnotations;

public class UpdateDoctorDto
{
    [Required]
    public string Code { get; set; } = null!;

<<<<<<< HEAD
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
=======
    [Required]
    public string Specialty { get; set; } = null!;

    public string? LicenseNumber { get; set; }
}
>>>>>>> origin/feature/doctor-frontend-MinhThu
