using System;
using System.ComponentModel.DataAnnotations;

namespace ClinicManagement.Api.Dtos.Billing
{
    public class UpdateBillingDto
    {
        [Required]
        public Guid AppointmentId { get; set; }

        [Range(0, 1)]
        public decimal? InsuranceCoverPercent { get; set; }

        public string? InsuranceCode { get; set; }

        public decimal Surcharge { get; set; } = 0m;

        public decimal Discount { get; set; } = 0m;
    }
}
