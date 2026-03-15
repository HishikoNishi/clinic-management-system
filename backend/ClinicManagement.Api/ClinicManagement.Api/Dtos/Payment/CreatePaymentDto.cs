using ClinicManagement.Api.Models;

namespace ClinicManagement.Api.Dtos.Payment
{
    public class CreatePaymentDto
    {
        public Guid InvoiceId { get; set; } //MaHoaDon
        public decimal Amount { get; set; } //SoTienThanhToan
        public PaymentMethod Method { get; set; } //Phương thức thanh toán (ví dụ: tiền mặt, thẻ tín dụng, chuyển khoản)
    }
}
