namespace ClinicManagement.Api.Models
{
    public class InsurancePlan
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Code { get; set; } = string.Empty; // Mã bệnh nhân đọc cho thu ngân
        public string Name { get; set; } = string.Empty;
        public decimal CoveragePercent { get; set; } = 0m; // 0-1
        public string? Note { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime? ExpiryDate { get; set; }
    }
}
