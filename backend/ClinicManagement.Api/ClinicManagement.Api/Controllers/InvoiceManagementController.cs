using ClinicManagement.Api.Data;
using ClinicManagement.Api.Dtos.Invoices;
using ClinicManagement.Api.Dtos.Billing;
using ClinicManagement.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ClinicManagement.Api.Services;

namespace ClinicManagement.Api.Controllers
{
    [Authorize(Roles = "Cashier,Admin")]
    [ApiController]
    [Route("api/[controller]")]
    public class InvoiceManagementController : ControllerBase
    {
        private readonly ClinicDbContext _context;
        private readonly BillingService _billingService;
        private readonly FakeInsuranceService _fakeInsuranceService;

        public InvoiceManagementController(ClinicDbContext context, BillingService billingService, FakeInsuranceService fakeInsuranceService)
        {
            _context = context;
            _billingService = billingService;
            _fakeInsuranceService = fakeInsuranceService;
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

            var existed = await _context.Invoices.AnyAsync(i => i.AppointmentId == dto.AppointmentId);
            if (existed)
                return BadRequest(new { message = "Lịch hẹn đã có hóa đơn" });

            var invoice = new Invoice
            {
                AppointmentId = dto.AppointmentId,
                Amount = dto.Amount,
                BalanceDue = dto.Amount,
                TotalDeposit = 0,
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
                    .ThenInclude(a => a.Patient)
                .Include(i => i.Payments)
                .Include(i => i.InvoiceLines)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (invoice == null)
                return NotFound(new { message = "Không tìm thấy hóa đơn" });

            return Ok(MapInvoice(invoice));
        }

        // ==============================
        // Danh sách hóa đơn (lọc trạng thái)
        // ==============================
        [HttpGet("list")]
        public async Task<IActionResult> List([FromQuery] bool? isPaid)
        {
            var query = _context.Invoices
                .AsNoTracking()
                .Include(i => i.Appointment).ThenInclude(a => a.Patient)
                .OrderByDescending(i => i.CreatedAt)
                .Select(i => new
                {
                    i.Id,
                    i.AppointmentId,
                    PatientName = i.Appointment != null ? i.Appointment.Patient.FullName : null,
                    i.Amount,
                    i.BalanceDue,
                    i.TotalDeposit,
                    i.IsPaid,
                    i.CreatedAt,
                    i.PaymentDate
                });

            if (isPaid.HasValue)
            {
                query = query.Where(i => i.IsPaid == isPaid.Value);
            }

            var items = await query.ToListAsync();
            return Ok(items);
        }

        // ==============================
        // Thanh toán hóa đơn (tương thích cũ, nhưng đồng thời tạo Payment)
        // ==============================
        [HttpPut("{id}/pay")]
        public async Task<IActionResult> PayInvoice(Guid id)
        {
            var invoice = await _context.Invoices.FindAsync(id);
            if (invoice == null)
                return NotFound(new { message = "Không tìm thấy hóa đơn" });

            if (invoice.IsPaid)
                return BadRequest(new { message = "Hóa đơn đã thanh toán" });

            if (invoice.Amount <= 0)
            {
                invoice.IsPaid = true;
                invoice.PaymentDate = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                return Ok(new { message = "Hóa đơn không còn số tiền phải thu" });
            }

            var payment = new Payment
            {
                InvoiceId = invoice.Id,
                AppointmentId = invoice.AppointmentId,
                Amount = invoice.Amount,
                DepositAmount = 0m,
                IsDeposit = false,
                Method = PaymentMethod.cash,
                PaymentDate = DateTime.UtcNow
            };

            invoice.IsPaid = true;
            invoice.PaymentDate = payment.PaymentDate;

            _context.Payments.Add(payment);

            await _context.SaveChangesAsync();
            return Ok(new
            {
                message = "Thanh toán thành công",
                invoice,
                payment
            });
        }

        // ==============================
        // Cập nhật bảo hiểm/phụ thu/giảm trừ và tính lại hóa đơn
        // ==============================
        [HttpPost("recalculate")]
        public async Task<IActionResult> Recalculate([FromBody] UpdateBillingDto dto)
        {
            if (dto.AppointmentId == Guid.Empty)
                return BadRequest(new { message = "Thiếu AppointmentId" });

            var record = await _context.MedicalRecords
                .FirstOrDefaultAsync(r => r.AppointmentId == dto.AppointmentId);

            if (record == null)
                return NotFound(new { message = "Không tìm thấy hồ sơ khám" });

            // cập nhật thông tin tài chính do cashier nhập
            var cover = dto.InsuranceCoverPercent;
            if (cover < 0) cover = 0;
            if (cover > 1) cover = 1;
            record.InsuranceCoverPercent = cover;
            record.Surcharge = dto.Surcharge;
            record.Discount = dto.Discount;

            if (!string.IsNullOrWhiteSpace(dto.InsuranceCode))
            {
                var plan = _fakeInsuranceService.Verify(dto.InsuranceCode);
                if (plan == null)
                    return BadRequest(new { message = "Bảo hiểm không hợp lệ hoặc đã hết hạn" });

                record.InsurancePlanCode = plan.Code;
                record.InsuranceCoverPercent = plan.CoveragePercent;
            }

            await _context.SaveChangesAsync();

            var invoice = await _billingService.GenerateInvoiceAsync(dto.AppointmentId);

            if (invoice == null)
                return NotFound(new { message = "Không tạo được hóa đơn" });

            var result = await _context.Invoices
                .Include(i => i.Appointment).ThenInclude(a => a.Patient)
                .Include(i => i.Payments)
                .Include(i => i.InvoiceLines)
                .FirstOrDefaultAsync(i => i.Id == invoice.Id);

            return Ok(MapInvoice(result!));
        }

        private object MapInvoice(Invoice invoice)
        {
            return new
            {
                invoice.Id,
                invoice.AppointmentId,
                PatientName = invoice.Appointment?.Patient?.FullName,
                invoice.Amount,
                invoice.TotalDeposit,
                invoice.BalanceDue,
                invoice.IsPaid,
                invoice.CreatedAt,
                invoice.PaymentDate,
                appointment = invoice.Appointment == null ? null : new
                {
                    invoice.Appointment.Id,
                    patient = invoice.Appointment.Patient == null ? null : new
                    {
                        invoice.Appointment.Patient.FullName,
                        invoice.Appointment.Patient.Phone,
                        invoice.Appointment.Patient.Email
                    }
                },
                payments = invoice.Payments?.Select(p => new
                {
                    p.Id,
                    p.Amount,
                    p.DepositAmount,
                    p.IsDeposit,
                    p.Method,
                    p.PaymentDate
                }),
                depositPayments = invoice.Payments?
                    .Where(p => p.IsDeposit)
                    .Select(p => new
                    {
                        p.Id,
                        p.Amount,
                        p.Method,
                        p.PaymentDate
                    }),
                invoiceLines = invoice.InvoiceLines?.Select(l => new
                {
                    l.Id,
                    l.Description,
                    l.ItemType,
                    l.Amount
                })
            };
        }
    }
}
