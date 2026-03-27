using System.ComponentModel.DataAnnotations;

namespace ClinicManagement.Api.Models
{
    public class MedicalRecord
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid AppointmentId { get; set; }

        public Guid DoctorId { get; set; }

        public Guid PatientId { get; set; }

        public string Symptoms { get; set; }

        public string Diagnosis { get; set; }

        public string Treatment { get; set; }

        public string Note { get; set; }

        // Mã gói bảo hiểm áp dụng (nếu có)
        public string? InsurancePlanCode { get; set; }

        // Bảo hiểm y tế chi trả (% 0-1); 0 nếu không có
        public decimal InsuranceCoverPercent { get; set; } = 0m;

        // Phụ thu/giảm trừ thủ công
        public decimal Surcharge { get; set; } = 0m;
        public decimal Discount { get; set; } = 0m;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
