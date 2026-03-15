using ClinicManagement.Api.Data;
using ClinicManagement.Api.Dtos.Invoices;
using ClinicManagement.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Api.Controllers
{
    //[Authorize(Roles = "Staff")]
    [ApiController]
    [Route("api/[controller]")]
    public class InvoiceManagementController : ControllerBase
    {
        private readonly ClinicDbContext _context;

        public InvoiceManagementController(ClinicDbContext context)
        {
            _context = context;
        }

        // ==============================
        // Tạo hóa đơn
        // ==============================
        [HttpPost]
        public async Task<IActionResult> CreateInvoice([FromBody] CreateInvoiceDto dto)
        {
            var appointment = await _context.Appointments.FindAsync(dto.AppointmentId);
            if (appointment == null)
                return NotFound(new { message = "Không tìm thấy lịch hẹn" });

            var invoice = new Invoice
            {
                AppointmentId = dto.AppointmentId,
                Amount = dto.Amount,
                CreatedAt = DateTime.UtcNow,
                IsPaid = false

            };

            _context.Invoices.Add(invoice);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Tạo hóa đơn thành công", invoiceId = invoice.Id });
        }

        // ==============================
        // Lấy chi tiết hóa đơn
        // ==============================
        [HttpGet("{id}")]
        public async Task<IActionResult> GetInvoice(Guid id)
        {
            var invoice = await _context.Invoices
                .Include(i => i.Appointment)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (invoice == null)
                return NotFound(new { message = "Không tìm thấy hóa đơn" });

            return Ok(invoice);
        }

        // ==============================
        // Thanh toán hóa đơn
        // ==============================
        [HttpPut("{id}/pay")]
        public async Task<IActionResult> PayInvoice(Guid id)
        {
            var invoice = await _context.Invoices.FindAsync(id);
            if (invoice == null)
                return NotFound(new { message = "Không tìm thấy hóa đơn" });

            if (invoice.IsPaid)
                return BadRequest(new { message = "Hóa đơn đã thanh toán" });

            invoice.IsPaid = true;
            invoice.PaymentDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return Ok(new { message = "Thanh toán thành công" });
        }
    }
}
