namespace ClinicManagement.Api.Models
{
    public class Invoice
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid AppointmentId { get; set; } //MaLichHen 
        public Appointment Appointment { get; set; } = null!; //LichHen 
        public decimal Amount { get; set; } //SoTien (số còn phải trả)
        public decimal TotalDeposit { get; set; } = 0m; // Tổng tạm ứng đã thu
        public decimal BalanceDue { get; set; } = 0m;   // Số còn phải thu sau khi trừ tạm ứng
        public bool IsPaid { get; set; } = false; //DaThanhToan
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow; //NgayTao 
        public DateTime? PaymentDate { get; set; } //NgayThanhToan
        public ICollection<Payment> Payments { get; set; } = new List<Payment>();
        public ICollection<InvoiceLine> InvoiceLines { get; set; } = new List<InvoiceLine>();
    }
}
