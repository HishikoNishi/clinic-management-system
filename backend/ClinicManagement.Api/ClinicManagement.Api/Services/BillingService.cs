using ClinicManagement.Api.Data;
using ClinicManagement.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace ClinicManagement.Api.Services
{
    public class BillingService
    {
        private readonly ClinicDbContext _context;
        private readonly IConfiguration _configuration;

        public BillingService(ClinicDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task<Invoice?> GenerateInvoiceAsync(Guid appointmentId)
        {
            var appointment = await _context.Appointments
                .Include(a => a.Patient)
                .FirstOrDefaultAsync(a => a.Id == appointmentId);

            if (appointment == null) return null;

            var medicalRecord = await _context.MedicalRecords
                .Where(m => m.AppointmentId == appointmentId)
                .OrderByDescending(m => m.CreatedAt)
                .FirstOrDefaultAsync();

            if (medicalRecord == null) return null;

            var prescription = await _context.Prescriptions
                .Include(p => p.PrescriptionDetails)
                .FirstOrDefaultAsync(p => p.MedicalRecordId == medicalRecord.Id);

            var tests = await _context.ClinicalTests
                .Where(t => t.MedicalRecordId == medicalRecord.Id)
                .ToListAsync();

            decimal subtotal = 0m;
            var lines = new List<InvoiceLine>();

            // Consultation fee
            var consultationFee = _configuration.GetValue<decimal?>("Billing:ConsultationFee") ?? 0m;
            if (consultationFee > 0)
            {
                subtotal += consultationFee;
                lines.Add(new InvoiceLine
                {
                    Description = "Phí khám",
                    ItemType = "Consultation",
                    Amount = consultationFee
                });
            }

            // Drugs
            if (prescription?.PrescriptionDetails != null)
            {
                foreach (var d in prescription.PrescriptionDetails)
                {
                    var price = GetDrugPrice(d.MedicineName);
                    if (price <= 0) continue;

                    var qty = d.Duration > 0 ? d.Duration : 1;
                    var amount = price * qty;
                    subtotal += amount;

                    lines.Add(new InvoiceLine
                    {
                        Description = $"Thuốc: {d.MedicineName}",
                        ItemType = "Drug",
                        Amount = amount
                    });
                }
            }

            // Clinical tests
            foreach (var t in tests)
            {
                var price = GetTestPrice(t.TestName);
                if (price <= 0) continue;
                subtotal += price;
                lines.Add(new InvoiceLine
                {
                    Description = $"Xét nghiệm: {t.TestName}",
                    ItemType = "Test",
                    Amount = price
                });
            }

            // Insurance discount
            var insuranceCover = medicalRecord.InsuranceCoverPercent;
            decimal insuranceDiscount = 0m;
            if (insuranceCover > 0 && insuranceCover <= 1)
            {
                insuranceDiscount = Math.Round(subtotal * insuranceCover, 2);
                if (insuranceDiscount > 0)
                {
                    lines.Add(new InvoiceLine
                    {
                        Description = $"Bảo hiểm chi trả ({insuranceCover:P0})",
                        ItemType = "Insurance",
                        Amount = -insuranceDiscount
                    });
                }
            }

            // Manual surcharge / discount
            if (medicalRecord.Surcharge != 0)
            {
                lines.Add(new InvoiceLine
                {
                    Description = "Phụ thu",
                    ItemType = "Surcharge",
                    Amount = medicalRecord.Surcharge
                });
            }

            if (medicalRecord.Discount != 0)
            {
                lines.Add(new InvoiceLine
                {
                    Description = "Giảm trừ",
                    ItemType = "Discount",
                    Amount = -Math.Abs(medicalRecord.Discount)
                });
            }

            var total = subtotal - insuranceDiscount + medicalRecord.Surcharge - Math.Abs(medicalRecord.Discount);
            if (total < 0) total = 0;

            // Deposits thu trước
            var deposits = await _context.Payments
                .Where(p => p.AppointmentId == appointmentId && p.IsDeposit)
                .ToListAsync();
            var totalDeposit = deposits.Sum(d => d.Amount);

            if (totalDeposit > 0)
            {
                lines.Add(new InvoiceLine
                {
                    Description = "Tạm ứng đã thu",
                    ItemType = "Deposit",
                    Amount = -totalDeposit
                });
            }

            var balanceDue = total - totalDeposit;
            if (balanceDue < 0) balanceDue = 0;
            var fullyCovered = balanceDue == 0;

            // Upsert invoice by appointment
            var invoice = await _context.Invoices
                .Include(i => i.InvoiceLines)
                .FirstOrDefaultAsync(i => i.AppointmentId == appointmentId);

            if (invoice == null)
            {
                invoice = new Invoice
                {
                    Id = Guid.NewGuid(),
                    AppointmentId = appointmentId,
                    Amount = balanceDue,
                    BalanceDue = balanceDue,
                    TotalDeposit = totalDeposit,
                    CreatedAt = DateTime.UtcNow,
                    IsPaid = fullyCovered,
                    PaymentDate = fullyCovered ? DateTime.UtcNow : null
                };
                _context.Invoices.Add(invoice);
            }
            else
            {
                if (invoice.IsPaid)
                {
                    // không tự động cập nhật hóa đơn đã thanh toán
                    return invoice;
                }
                invoice.Amount = balanceDue;
                invoice.BalanceDue = balanceDue;
                invoice.TotalDeposit = totalDeposit;
                invoice.PaymentDate = fullyCovered ? invoice.PaymentDate ?? DateTime.UtcNow : null;
                invoice.IsPaid = fullyCovered;

                // refresh lines
                _context.InvoiceLines.RemoveRange(invoice.InvoiceLines);
                invoice.InvoiceLines.Clear();
            }

            foreach (var line in lines)
            {
                line.InvoiceId = invoice.Id;
                _context.InvoiceLines.Add(line);
            }

            // gán invoiceId cho các khoản tạm ứng chưa gán
            foreach (var deposit in deposits.Where(d => d.InvoiceId == null))
            {
                deposit.InvoiceId = invoice.Id;
            }

            await _context.SaveChangesAsync();
            return invoice;
        }

        private decimal GetDrugPrice(string medicineName)
        {
            var list = _configuration.GetSection("Billing:DrugPrices").Get<List<NamedPrice>>() ?? new();
            var price = list.FirstOrDefault(x => string.Equals(x.Name, medicineName, StringComparison.OrdinalIgnoreCase))?.Price ?? 0m;
            return price;
        }

        private decimal GetTestPrice(string testName)
        {
            var list = _configuration.GetSection("Billing:TestPrices").Get<List<NamedPrice>>() ?? new();
            var price = list.FirstOrDefault(x => string.Equals(x.Name, testName, StringComparison.OrdinalIgnoreCase))?.Price ?? 0m;
            return price;
        }

        private class NamedPrice
        {
            public string Name { get; set; } = string.Empty;
            public decimal Price { get; set; }
        }
    }
}

