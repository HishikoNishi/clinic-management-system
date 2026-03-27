using System;
using ClinicManagement.Api.Models;

namespace ClinicManagement.Api.Models
{
    public class Payment
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        //Khóa ngoại đến Invoice
        public Guid InvoiceId { get; set; } //MaHoaDon
        public Invoice Invoice { get; set; } = null!; //HoaDon

        //Số tiền thanh toán
        public decimal Amount { get; set; } //SoTienThanhToan

        //Phương thức thanh toán (ví dụ: tiền mặt, thẻ tín dụng, chuyển khoản)
        public PaymentMethod Method { get; set; } 

        //thời gian thanh toán
        public DateTime PaymentDate { get; set; } = DateTime.UtcNow; //NgayThanhToan
    }
}
