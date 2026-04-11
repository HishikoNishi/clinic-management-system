using System.ComponentModel.DataAnnotations;

namespace ClinicManagement.Api.Dtos.Medicines
{
    public class UpsertMedicineDto
    {
        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(50)]
        public string? DefaultDosage { get; set; }

        [Required]
        [MaxLength(30)]
        public string Unit { get; set; } = "Vien";

        [Range(0, 1000000000)]
        public decimal Price { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
