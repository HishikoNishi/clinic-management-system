using System;
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
        // Táº¡o hÃ³a Ä‘Æ¡n
        // ==============================
        [HttpPost]
        public async Task<IActionResult> CreateInvoice([FromBody] CreateInvoiceDto dto)
        {
            var appointment = await _context.Appointments.FindAsync(dto.AppointmentId);
            if (appointment == null)
                return NotFound(new { message = "KhÃ´ng tÃ¬m tháº¥y lá»‹ch háº¹n" });

            var existed = await _context.Invoices.AnyAsync(i => i.AppointmentId == dto.AppointmentId);
            if (existed)
                return BadRequest(new { message = "Lá»‹ch háº¹n Ä‘Ã£ cÃ³ hÃ³a Ä‘Æ¡n" });

            var invoice = new Invoice
            {
                AppointmentId = dto.AppointmentId,
                Amount = dto.Amount,
                BalanceDue = dto.Amount,
                TotalDeposit = 0,
                CreatedAt = DateTime.UtcNow,
                IsPaid = false,
                InvoiceType = InvoiceType.Clinic

            };

            _context.Invoices.Add(invoice);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Táº¡o hÃ³a Ä‘Æ¡n thÃ nh cÃ´ng", invoiceId = invoice.Id });
        }

        // ==============================
        // Láº¥y chi tiáº¿t hÃ³a Ä‘Æ¡n
        // ==============================
        [HttpGet("{id}")]
        public async Task<IActionResult> GetInvoice(Guid id)
        {
            var invoice = await _context.Invoices
                .AsNoTracking()
                .Include(i => i.Appointment)
                    .ThenInclude(a => a.Patient)
                .Include(i => i.Payments)
                .Include(i => i.InvoiceLines)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (invoice == null)
                return NotFound(new { message = "KhÃ´ng tÃ¬m tháº¥y hÃ³a Ä‘Æ¡n" });

            var record = await _context.MedicalRecords
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.AppointmentId == invoice.AppointmentId);

            return Ok(MapInvoice(invoice, record));
        }

        // ==============================
        // Danh sÃ¡ch hÃ³a Ä‘Æ¡n (lá»c tráº¡ng thÃ¡i)
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
                    i.PrescriptionId,
                    InvoiceType = i.InvoiceType.ToString(),
                    AppointmentCode = i.Appointment != null ? i.Appointment.AppointmentCode : null,
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
        // Thanh toÃ¡n hÃ³a Ä‘Æ¡n (tÆ°Æ¡ng thÃ­ch cÅ©, nhÆ°ng Ä‘á»“ng thá»i táº¡o Payment)
        // ==============================
        [Obsolete("Use POST /payment", false)]
        [ApiExplorerSettings(IgnoreApi = true)]
        [HttpPut("{id}/pay")]
        public async Task<IActionResult> PayInvoice(Guid id)
        {
            var invoice = await _context.Invoices.FindAsync(id);
            if (invoice == null)
                return NotFound(new { message = "KhÃ´ng tÃ¬m tháº¥y hÃ³a Ä‘Æ¡n" });

            if (invoice.IsPaid)
                return BadRequest(new { message = "HÃ³a Ä‘Æ¡n Ä‘Ã£ thanh toÃ¡n" });

            if (invoice.Amount <= 0)
            {
                invoice.IsPaid = true;
                invoice.PaymentDate = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                return Ok(new { message = "HÃ³a Ä‘Æ¡n khÃ´ng cÃ²n sá»‘ tiá»n pháº£i thu" });
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
                message = "Thanh toÃ¡n thÃ nh cÃ´ng",
                invoice,
                payment
            });
        }

        // ==============================
        // Cáº­p nháº­t báº£o hiá»ƒm/phá»¥ thu/giáº£m trá»« vÃ  tÃ­nh láº¡i hÃ³a Ä‘Æ¡n
        // ==============================
        [HttpPost("recalculate")]
        public async Task<IActionResult> Recalculate([FromBody] UpdateBillingDto dto)
        {
            if (dto.AppointmentId == Guid.Empty)
                return BadRequest(new { message = "Thiáº¿u AppointmentId" });

            var record = await _context.MedicalRecords
                .FirstOrDefaultAsync(r => r.AppointmentId == dto.AppointmentId);

            if (record == null)
                return NotFound(new { message = "KhÃ´ng tÃ¬m tháº¥y há»“ sÆ¡ khÃ¡m" });

            // cáº­p nháº­t thÃ´ng tin tÃ i chÃ­nh do cashier nháº­p
            if (dto.InsuranceCoverPercent.HasValue)
            {
                var cover = dto.InsuranceCoverPercent.Value;
                record.InsuranceCoverPercent = FinanceHelper.Clamp01(cover);
            }
            record.Surcharge = dto.Surcharge;
            record.Discount = dto.Discount;

            if (!string.IsNullOrWhiteSpace(dto.InsuranceCode))
            {
                var plan = _fakeInsuranceService.Verify(dto.InsuranceCode);
                if (plan == null)
                    return BadRequest(new { message = "Báº£o hiá»ƒm khÃ´ng há»£p lá»‡ hoáº·c Ä‘Ã£ háº¿t háº¡n" });

                record.InsurancePlanCode = plan.Code;
                record.InsuranceCoverPercent = FinanceHelper.Clamp01(plan.CoveragePercent);
            }

            await _context.SaveChangesAsync();

            var invoice = await _billingService.GenerateInvoiceAsync(dto.AppointmentId);

            if (invoice == null)
                return NotFound(new { message = "KhÃ´ng táº¡o Ä‘Æ°á»£c hÃ³a Ä‘Æ¡n" });

            var result = await _context.Invoices
                .AsNoTracking()
                .Include(i => i.Appointment).ThenInclude(a => a.Patient)
                .Include(i => i.Payments)
                .Include(i => i.InvoiceLines)
                .FirstOrDefaultAsync(i => i.Id == invoice.Id);

            return Ok(MapInvoice(result!, record));
        }

        // ==============================
        // HÃ³a Ä‘Æ¡n thuá»‘c
        // ==============================
        public class CreateDrugInvoiceDto
        {
            public Guid PrescriptionId { get; set; }
        }

        [HttpPost("drug")]
        public async Task<IActionResult> CreateDrugInvoice([FromBody] CreateDrugInvoiceDto dto)
        {
            if (dto.PrescriptionId == Guid.Empty)
                return BadRequest(new { message = "Thiáº¿u PrescriptionId" });
            var invoice = await _billingService.GenerateDrugInvoiceAsync(dto.PrescriptionId);
            if (invoice == null) return NotFound(new { message = "KhÃ´ng táº¡o Ä‘Æ°á»£c hÃ³a Ä‘Æ¡n thuá»‘c" });

            var result = await _context.Invoices
                .AsNoTracking()
                .Include(i => i.Appointment).ThenInclude(a => a.Patient)
                .Include(i => i.InvoiceLines)
                .FirstOrDefaultAsync(i => i.Id == invoice.Id);

            return Ok(MapInvoice(result!, null));
        }

        [HttpGet("drug/by-prescription/{id:guid}")]
        public async Task<IActionResult> GetDrugInvoiceByPrescription(Guid id)
        {
            var invoice = await _context.Invoices
                .AsNoTracking()
                .Include(i => i.Appointment).ThenInclude(a => a.Patient)
                .Include(i => i.InvoiceLines)
                .FirstOrDefaultAsync(i => i.PrescriptionId == id && i.InvoiceType == InvoiceType.Drug);

            if (invoice == null) return NotFound(new { message = "KhÃ´ng tÃ¬m tháº¥y hÃ³a Ä‘Æ¡n thuá»‘c" });
            return Ok(MapInvoice(invoice, null));
        }

        private object MapInvoice(Invoice invoice, MedicalRecord? med)
        {
            var insuranceCover = med?.InsuranceCoverPercent ?? 0m;
            string? insurancePlan = med?.InsurancePlanCode;
            return new
            {
                invoice.Id,
                invoice.AppointmentId,
                invoice.PrescriptionId,
                InvoiceType = invoice.InvoiceType.ToString(),
                AppointmentCode = invoice.Appointment?.AppointmentCode,
                PatientName = invoice.Appointment?.Patient?.FullName,
                invoice.Amount,
                invoice.TotalDeposit,
                invoice.BalanceDue,
                invoice.IsPaid,
                invoice.CreatedAt,
                invoice.PaymentDate,
                InsuranceCoverPercent = insuranceCover,
                InsurancePlanCode = insurancePlan,
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
                    l.Amount,
                    l.Dosage,   
                    l.Duration
                })
            };
        }
    }
}

