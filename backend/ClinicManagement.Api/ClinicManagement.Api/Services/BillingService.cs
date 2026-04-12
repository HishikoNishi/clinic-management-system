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

        public async Task<Invoice?> GenerateDrugInvoiceAsync(Guid prescriptionId)
        {
            var prescription = await _context.Prescriptions
                .Include(p => p.PrescriptionDetails)
                .FirstOrDefaultAsync(p => p.Id == prescriptionId);
            if (prescription == null) return null;

            var medicalRecord = await _context.MedicalRecords
                .FirstOrDefaultAsync(m => m.Id == prescription.MedicalRecordId);
            if (medicalRecord == null) return null;

            var appointmentId = medicalRecord.AppointmentId;

            decimal subtotal = 0m;
            var lines = new List<InvoiceLine>();

            // Prefer medicine catalog prices (admin-managed) over billing.json (fallback).
            var medicineCatalog = await _context.Medicines
                .AsNoTracking()
                .Select(m => new { m.Id, m.Name, m.Price })
                .ToListAsync();

            if (prescription.PrescriptionDetails != null)
            {
                foreach (var d in prescription.PrescriptionDetails)
                {
                    var qty = d.TotalQuantity > 0 ? d.TotalQuantity : (d.Duration > 0 ? d.Duration : 1);

                    decimal unitPrice = 0m;
                    if (d.UnitPrice > 0)
                    {
                        unitPrice = d.UnitPrice;
                    }
                    else if (d.MedicineId.HasValue)
                    {
                        unitPrice = medicineCatalog.FirstOrDefault(m => m.Id == d.MedicineId.Value)?.Price ?? 0m;
                    }

                    if (unitPrice <= 0)
                    {
                        var medName = (d.MedicineName ?? string.Empty).Trim();

                        // 1) Exact match by name
                        unitPrice = medicineCatalog
                            .FirstOrDefault(m => string.Equals(m.Name, medName, StringComparison.OrdinalIgnoreCase))
                            ?.Price ?? 0m;

                        // 2) Prefix match: "Paracetamol 500mg" -> "Paracetamol"
                        if (unitPrice <= 0 && !string.IsNullOrWhiteSpace(medName))
                        {
                            var match = medicineCatalog
                                .Where(m =>
                                    !string.IsNullOrWhiteSpace(m.Name) &&
                                    medName.StartsWith(m.Name, StringComparison.OrdinalIgnoreCase) &&
                                    (medName.Length == m.Name.Length ||
                                     char.IsWhiteSpace(medName[m.Name.Length]) ||
                                     medName[m.Name.Length] == '-' ||
                                     medName[m.Name.Length] == '('))
                                .OrderByDescending(m => m.Name.Length)
                                .FirstOrDefault();
                            unitPrice = match?.Price ?? 0m;
                        }
                    }

                    if (unitPrice <= 0)
                    {
                        // Fallback to billing.json config
                        unitPrice = _pricing.GetDrugPrice(d.MedicineName);
                    }

                    var amount = unitPrice * qty;
                    subtotal += amount;
                    lines.Add(new InvoiceLine
                    {
                        Description = unitPrice > 0
                            ? $"Thuoc: {d.MedicineName}"
                            : $"Thuoc: {d.MedicineName} (chua cau hinh gia)",
                        ItemType = "Drug",
                        Amount = amount
                    });
                }
            }

            var insuranceCover = medicalRecord.InsuranceCoverPercent;
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

            var total = subtotal - insuranceDiscount;
            if (total < 0) total = 0;

            // Upsert drug invoice by prescription
            var invoice = await _context.Invoices
                .Include(i => i.InvoiceLines)
                .FirstOrDefaultAsync(i => i.PrescriptionId == prescriptionId && i.InvoiceType == InvoiceType.Drug);

            if (invoice == null)
            {
                invoice = new Invoice
                {
                    Id = Guid.NewGuid(),
                    AppointmentId = appointmentId,
                    PrescriptionId = prescriptionId,
                    InvoiceType = InvoiceType.Drug,
                    Amount = total,
                    BalanceDue = total,
                    TotalDeposit = 0,
                    CreatedAt = DateTime.UtcNow,
                    IsPaid = false,
                    PaymentDate = null
                };
                _context.Invoices.Add(invoice);
            }
            else
            {
                if (invoice.IsPaid) return invoice; // không sửa hóa đơn đã thu
                invoice.Amount = total;
                invoice.BalanceDue = total;

                _context.InvoiceLines.RemoveRange(invoice.InvoiceLines);
                invoice.InvoiceLines.Clear();
            }

            foreach (var line in lines)
            {
                line.InvoiceId = invoice.Id;
                _context.InvoiceLines.Add(line);
            }

            await _context.SaveChangesAsync();
            return invoice;
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

            // Clinic invoice ONLY: bỏ thuốc, chỉ tính khám + xét nghiệm + bảo hiểm + tạm ứng
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
                balanceDue = 0;
            }

            // Upsert invoice by appointment (Clinic type)
            var invoice = await _context.Invoices
                .Include(i => i.InvoiceLines)
                .FirstOrDefaultAsync(i => i.AppointmentId == appointmentId && i.InvoiceType == InvoiceType.Clinic);

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
                    PaymentDate = null,
                    InvoiceType = InvoiceType.Clinic
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
