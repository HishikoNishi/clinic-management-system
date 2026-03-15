using ClinicManagement.Api.Data;
using ClinicManagement.Api.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ClinicManagement.Api.Dtos.Payment;

namespace ClinicManagement.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly ClinicDbContext _context;
        public PaymentController(ClinicDbContext context)
        {
            _context = context;
        }

        // POST: api/payment
        [HttpPost]
        public async Task<IActionResult> CreatePayment(CreatePaymentDto dto)
        {
            // Kiểm tra hóa đơn tồn tại
            var invoice = await _context.Invoices
                .FirstOrDefaultAsync(i => i.Id == dto.InvoiceId);

            if (invoice == null) 
                return NotFound("Hóa đơn không tồn tại");

            if (invoice.IsPaid)
                return BadRequest("Hóa đơn đã được thanh toán");

            // Tạo đối tượng Payment mới
            var payment = new Payment
            {
                Id = Guid.NewGuid(),
                InvoiceId = dto.InvoiceId,
                Amount = dto.Amount,
                Method = dto.Method,
                PaymentDate = DateTime.UtcNow
            };

            invoice.IsPaid = true;
            invoice.PaymentDate = payment.PaymentDate;

            // Lưu vào cơ sở dữ liệu
            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                Message = "Thanh toán thành công",
                Payment = new
                {
                    payment.Id,
                    payment.InvoiceId,
                    payment.Amount,
                    payment.Method,
                    payment.PaymentDate
                },
                Invoice = new
                {
                    invoice.Id,
                    invoice.Amount,
                    invoice.IsPaid,
                    invoice.PaymentDate
                }
            });

        }
    }
}
