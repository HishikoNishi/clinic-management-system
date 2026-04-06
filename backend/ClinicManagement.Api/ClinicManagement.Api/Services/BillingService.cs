using ClinicManagement.Api.Data;
using ClinicManagement.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Api.Services
{
    /// <summary>
    /// Tinh hoa don tu ho so kham, don thuoc, xet nghiem, bao hiem va tam ung.
    /// </summary>
    public class BillingService
    {
        private readonly ClinicDbContext _context;
        private readonly IPricingProvider _pricing;

        public BillingService(ClinicDbContext context, IPricingProvider pricing)
        {
            _context = context;
            _pricing = pricing;
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

            var insuranceCover = medicalRecord.InsuranceCoverPercent;

            // Clinic ticket: free with insurance, otherwise charge clinic ticket fee (fallback consultation fee)
            var consultationFee = insuranceCover > 0
                ? 0m
                : (_pricing.ClinicTicketFee > 0 ? _pricing.ClinicTicketFee : _pricing.ConsultationFee);
            if (consultationFee > 0)
            {
                subtotal += consultationFee;
                lines.Add(new InvoiceLine
                {
                    Description = "Phieu kham",
                    ItemType = "Consultation",
                    Amount = consultationFee
                });
            }

            // Drugs
            if (prescription?.PrescriptionDetails != null)
            {
                foreach (var d in prescription.PrescriptionDetails)
                {
                    var price = _pricing.GetDrugPrice(d.MedicineName);
                    if (price <= 0) continue;

                    var qty = d.Duration > 0 ? d.Duration : 1;
                    var amount = price * qty;
                    subtotal += amount;

                    lines.Add(new InvoiceLine
                    {
                        Description = $"Thuoc: {d.MedicineName}",
                        ItemType = "Drug",
                        Amount = amount
                    });
                }
            }

            // Clinical tests
            foreach (var t in tests)
            {
                var price = _pricing.GetTestPrice(t.TestName);
                if (price <= 0) continue;
                subtotal += price;
                lines.Add(new InvoiceLine
                {
                    Description = $"Xet nghiem: {t.TestName}",
                    ItemType = "Test",
                    Amount = price
                });
            }

            // Insurance discount
            decimal insuranceDiscount = 0m;
            if (insuranceCover > 0 && insuranceCover <= 1)
            {
                insuranceDiscount = Math.Round(subtotal * insuranceCover, 2);
                if (insuranceDiscount > 0)
                {
                    lines.Add(new InvoiceLine
                    {
                        Description = $"Bao hiem chi tra ({insuranceCover:P0})",
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
                    Description = "Phu thu",
                    ItemType = "Surcharge",
                    Amount = medicalRecord.Surcharge
                });
            }

            if (medicalRecord.Discount != 0)
            {
                lines.Add(new InvoiceLine
                {
                    Description = "Giam tru",
                    ItemType = "Discount",
                    Amount = -Math.Abs(medicalRecord.Discount)
                });
            }

            var subtotalAfterInsurance = subtotal - insuranceDiscount + medicalRecord.Surcharge - Math.Abs(medicalRecord.Discount);
            if (subtotalAfterInsurance < 0) subtotalAfterInsurance = 0;

            // Deposits thu truoc
            var deposits = await _context.Payments
                .Where(p => p.AppointmentId == appointmentId && p.IsDeposit)
                .ToListAsync();
            var totalDepositCollected = deposits.Sum(d => d.Amount);

            // khau tru toi da bang subtotalAfterInsurance
            var appliedDeposit = Math.Min(totalDepositCollected, subtotalAfterInsurance);

            if (appliedDeposit > 0)
            {
                var desc = totalDepositCollected > appliedDeposit
                    ? $"Tam ung da thu (ap dung {appliedDeposit:N0}/{totalDepositCollected:N0})"
                    : "Tam ung da thu";

                lines.Add(new InvoiceLine
                {
                    Description = desc,
                    ItemType = "Deposit",
                    Amount = -appliedDeposit
                });
            }

            var balanceDue = subtotalAfterInsurance - appliedDeposit;
            if (balanceDue < 0) balanceDue = 0;

            // tien du tam ung (refund)
            var depositRefund = totalDepositCollected - appliedDeposit;
            if (depositRefund > 0)
            {
                lines.Add(new InvoiceLine
                {
                    Description = "Hoan tam ung du",
                    ItemType = "DepositRefund",
                    Amount = -depositRefund
                });
                balanceDue = 0; // da hoan phan du, khong con phai thu
            }

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
                    Amount = subtotalAfterInsurance,
                    BalanceDue = balanceDue,
                    TotalDeposit = totalDepositCollected,
                    CreatedAt = DateTime.UtcNow,
                    IsPaid = false,
                    PaymentDate = null
                };
                _context.Invoices.Add(invoice);
            }
            else
            {
                if (invoice.IsPaid)
                {
                    // khong tu dong cap nhat hoa don da thanh toan
                    return invoice;
                }
                invoice.Amount = subtotalAfterInsurance;
                invoice.BalanceDue = balanceDue;
                invoice.TotalDeposit = totalDepositCollected;

                // refresh lines
                _context.InvoiceLines.RemoveRange(invoice.InvoiceLines);
                invoice.InvoiceLines.Clear();
            }

            foreach (var line in lines)
            {
                line.InvoiceId = invoice.Id;
                _context.InvoiceLines.Add(line);
            }

            // gan invoiceId cho cac khoan tam ung chua gan
            foreach (var deposit in deposits.Where(d => d.InvoiceId == null))
            {
                deposit.InvoiceId = invoice.Id;
            }

            await _context.SaveChangesAsync();
            return invoice;
        }
    }
}
