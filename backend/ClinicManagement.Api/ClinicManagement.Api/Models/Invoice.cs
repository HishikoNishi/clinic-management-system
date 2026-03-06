namespace ClinicManagement.Api.Models
{
    public class Invoice
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid AppointmentId { get; set; } //MaLichHen 
        public Appointment Appointment { get; set; } = null!; //LichHen 
        public decimal Amount { get; set; } //SoTien 
        public bool IsPaid { get; set; } = false; //DaThanhToan
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow; //NgayTao 
        public DateTime? PaymentDate { get; set; } //NgayThanhToan 
    }
}
