using System;
using System.ComponentModel.DataAnnotations;

namespace ClinicManagement.Api.Dtos.Specialty
{
    public class UpdateSpecialtyDto
    {
        [Required]
        public Guid DepartmentId { get; set; }

        [Required]
        [MaxLength(150)]
        public string Name { get; set; } = string.Empty;

    // no description needed
}
}
