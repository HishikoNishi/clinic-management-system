namespace ClinicManagement.Api.Models
{
    public class HoaDon
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid MaLichHen { get; set; }
        public Appointment LichHen { get; set; } = null!;
        public decimal SoTien { get; set; }
        public bool DaThanhToan { get; set; } = false;
        public DateTime NgayTao { get; set; } = DateTime.UtcNow;
        public DateTime? NgayThanhToan { get; set; }
    }
}
