using ClinicManagement.Api.Data;
using ClinicManagement.Api.Dtos.Invoices;
using ClinicManagement.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Api.Controllers
{
    [Authorize(Roles = "Staff")]
    [ApiController]
    [Route("api/invoices")]
    public class InvoiceManagementController : ControllerBase
    {
        private readonly ClinicDbContext _context;

        public InvoiceManagementController(ClinicDbContext context)
        {
            _context = context;
        }

        // ==============================
        // 4?? T?o hoá ??n
        // ==============================
        [HttpPost]
        public async Task<IActionResult> CreateInvoice([FromBody] CreateInvoiceDto dto)
        {
            var appointment = await _context.Appointments.FindAsync(dto.AppointmentId);
            if (appointment == null)
                return NotFound(new { message = "Không tìm th?y l?ch h?n" });

            var invoice = new Invoice
            {
                Id = Guid.NewGuid(),
                AppointmentId = dto.AppointmentId,
                Amount = dto.Amount,
                CreatedAt = DateTime.UtcNow,
                IsPaid = false
            };

            _context.Invoices.Add(invoice);
            await _context.SaveChangesAsync();

            return Ok(new { message = "T?o hoá ??n thành công", invoiceId = invoice.Id });
        }

        // ==============================
        // 5?? Thanh toán hoá ??n
        // ==============================
        [HttpPut("{id}/pay")]
        public async Task<IActionResult> PayInvoice(Guid id)
        {
            var invoice = await _context.Invoices.FindAsync(id);
            if (invoice == null)
                return NotFound(new { message = "Không tìm th?y hoá ??n" });

            if (invoice.IsPaid)
                return BadRequest(new { message = "Hoá ??n ?ã thanh toán" });

            invoice.IsPaid = true;
            invoice.PaymentDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return Ok(new { message = "Thanh toán thành công" });
        }
    }
}
