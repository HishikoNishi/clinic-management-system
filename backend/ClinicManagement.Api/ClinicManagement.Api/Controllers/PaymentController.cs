using ClinicManagement.Api.Data;
using ClinicManagement.Api.Dtos.Payment;
using ClinicManagement.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Api.Controllers
{
    [Authorize(Roles = "Cashier,Admin")]
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
            if (dto.Amount <= 0)
                return BadRequest("Số tiền thanh toán không hợp lệ");

            var invoice = await _context.Invoices
                .Include(i => i.Payments)
                .FirstOrDefaultAsync(i => i.Id == dto.InvoiceId);

            if (invoice == null)
                return NotFound("Hóa đơn không tồn tại");

            if (invoice.IsPaid)
                return BadRequest("Hóa đơn đã được thanh toán");

            // Đảm bảo thanh toán đúng số tiền hóa đơn (tránh thiếu/nhầm)
            if (Math.Round(dto.Amount, 2) != Math.Round(invoice.Amount, 2))
                return BadRequest("Số tiền không khớp với hóa đơn");

            var payment = new Payment
            {
                InvoiceId = dto.InvoiceId,
                Amount = dto.Amount,
                Method = dto.Method,
                PaymentDate = DateTime.UtcNow
            };

            invoice.IsPaid = true;
            invoice.PaymentDate = payment.PaymentDate;

            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();

            // nạp lại invoice kèm lines để UI hiển thị breakdown
            var invoiceWithLines = await _context.Invoices
                .Include(i => i.InvoiceLines)
                .FirstOrDefaultAsync(i => i.Id == invoice.Id);

            return Ok(new
            {
                message = "Thanh toán thành công",
                payment = new
                {
                    payment.Id,
                    payment.InvoiceId,
                    payment.Amount,
                    payment.Method,
                    payment.PaymentDate
                },
                invoice = invoiceWithLines ?? invoice
            });

        }
    }
}
