using System;
using ClinicManagement.Api.Models;

namespace ClinicManagement.Api.Models
{
    public class Payment
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        // Khóa ngoại đến Invoice (có thể null nếu thu tạm ứng trước khi tạo invoice)
        public Guid? InvoiceId { get; set; } //MaHoaDon
        public Invoice? Invoice { get; set; } //HoaDon

        // Liên kết lịch hẹn để thu tạm ứng trước khi có invoice
        public Guid AppointmentId { get; set; }
        public Appointment Appointment { get; set; } = null!;

        //Số tiền thanh toán
        public decimal Amount { get; set; } //SoTienThanhToan

        // Số tiền tạm ứng (để phân biệt khi Amount dùng chung)
        public decimal DepositAmount { get; set; } = 0m;

        // Đánh dấu đây là khoản tạm ứng
        public bool IsDeposit { get; set; } = false;

        //Phương thức thanh toán (ví dụ: tiền mặt, thẻ tín dụng, chuyển khoản)
        public PaymentMethod Method { get; set; } 

        //thời gian thanh toán
        public DateTime PaymentDate { get; set; } = DateTime.UtcNow; //NgayThanhToan
    }
}
