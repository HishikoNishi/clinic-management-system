using System;
using System.ComponentModel.DataAnnotations;

namespace ClinicManagement.Api.Models
{
    public class PrescriptionDetail
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid PrescriptionId { get; set; }

        public string MedicineName { get; set; }

        public string Dosage { get; set; }

        public int Quantity { get; set; }

        public Prescription? Prescription { get; set; }
    }
}