using ClinicManagement.Api.Data;
using ClinicManagement.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Api.Services
{
    public class BillingService
    {
        private readonly ClinicDbContext _context;
        private readonly IPricingProvider _pricing;

        public BillingService(ClinicDbContext context, IPricingProvider pricing)
        {
            _context = context;
            _pricing = pricing;
        }

        /// <summary>
        /// Tạo hoặc cập nhật hóa đơn thuốc dựa trên đơn thuốc (Prescription).
        /// Logic: Ưu tiên lấy giá từ danh mục Medicine, sau đó mới dùng bảng giá cấu hình.
        /// </summary>
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

            // Lấy danh mục thuốc hiện có để truy xuất giá gốc
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
                    // 1. Ưu tiên giá đã lưu trực tiếp trên chi tiết đơn thuốc
                    if (d.UnitPrice > 0)
                    {
                        unitPrice = d.UnitPrice;
                    }
                    // 2. Tìm theo MedicineId trong danh mục
                    else if (d.MedicineId.HasValue)
                    {
                        unitPrice = medicineCatalog.FirstOrDefault(m => m.Id == d.MedicineId.Value)?.Price ?? 0m;
                    }

                    // 3. Nếu vẫn chưa có giá, tìm kiếm theo tên thuốc (Exact match & Prefix match)
                    if (unitPrice <= 0)
                    {
                        var medName = (d.MedicineName ?? string.Empty).Trim();

                        unitPrice = medicineCatalog
                            .FirstOrDefault(m => string.Equals(m.Name, medName, StringComparison.OrdinalIgnoreCase))
                            ?.Price ?? 0m;

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

                    // 4. Cuối cùng, fallback về cấu hình pricing cố định
                    if (unitPrice <= 0)
                    {
                        unitPrice = _pricing.GetDrugPrice(d.MedicineName);
                    }

                    var amount = unitPrice * qty;
                    subtotal += amount;
                    lines.Add(new InvoiceLine
                    {
                        Description = $"Thuốc: {d.MedicineName}",
                        ItemType = "Drug",
                        Amount = amount,
                        Duration = d.Duration > 0 ? d.Duration : 1,
                        Dosage = d.Dosage
                    });
                }
            }

            // Tính toán chiết khấu từ bảo hiểm cho đơn thuốc
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

            var total = subtotal - insuranceDiscount;
            if (total < 0) total = 0;

            // Xử lý Upsert (Cập nhật nếu có, tạo mới nếu chưa) hóa đơn thuốc
            var invoice = await _context.Invoices
                .Include(i => i.InvoiceLines)
                .FirstOrDefaultAsync(i => i.AppointmentId == appointmentId && i.InvoiceType == InvoiceType.Drug);

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
                    PaymentDate = null,
                };
                _context.Invoices.Add(invoice);
            }
            else
            {
                // Nếu hóa đơn đã thanh toán, không cho phép cập nhật lại nội dung
                if (invoice.IsPaid) return invoice;

                invoice.PrescriptionId = prescriptionId;
                invoice.Amount = total;
                invoice.BalanceDue = total;

                // Xóa các dòng chi tiết cũ để nạp lại dữ liệu mới tránh trùng lặp
                if (invoice.InvoiceLines != null && invoice.InvoiceLines.Any())
                {
                    _context.InvoiceLines.RemoveRange(invoice.InvoiceLines);
                    invoice.InvoiceLines.Clear();
                }
            }

            foreach (var line in lines)
            {
                line.InvoiceId = invoice.Id;
                _context.InvoiceLines.Add(line);
            }

            await _context.SaveChangesAsync();
            return invoice;
        }

        /// <summary>
        /// Tạo hóa đơn phòng khám (Clinic Invoice) bao gồm phí khám, xét nghiệm, phụ thu và khấu trừ tạm ứng.
        /// Lưu ý: Hóa đơn này không bao gồm chi phí thuốc.
        /// </summary>
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

            // Lấy danh sách các xét nghiệm lâm sàng đã chỉ định
            var tests = await _context.ClinicalTests
                .Where(t => t.MedicalRecordId == medicalRecord.Id)
                .ToListAsync();

            decimal subtotal = 0m;
            var lines = new List<InvoiceLine>();
            var insuranceCover = medicalRecord.InsuranceCoverPercent;

            // Tính phí khám (Miễn phí nếu có bảo hiểm, hoặc lấy phí theo bảng giá)
            var consultationFee = insuranceCover > 0
                ? 0m
                : (_pricing.ClinicTicketFee > 0 ? _pricing.ClinicTicketFee : _pricing.ConsultationFee);
            if (consultationFee > 0)
            {
                subtotal += consultationFee;
                lines.Add(new InvoiceLine
                {
                    Description = "Phiếu khám",
                    ItemType = "Consultation",
                    Amount = consultationFee
                });
            }

            // Tính phí các xét nghiệm
            foreach (var t in tests)
            {
                var price = _pricing.GetTestPrice(t.TestName);
                if (price <= 0) continue;
                subtotal += price;
                lines.Add(new InvoiceLine
                {
                    Description = $"Xét nghiệm: {t.TestName}",
                    ItemType = "Test",
                    Amount = price
                });
            }

            // Khấu trừ bảo hiểm trên tổng phí dịch vụ phòng khám
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

            // Xử lý các khoản phụ thu hoặc giảm giá thủ công
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

            var subtotalAfterInsurance = subtotal - insuranceDiscount + medicalRecord.Surcharge - Math.Abs(medicalRecord.Discount);
            if (subtotalAfterInsurance < 0) subtotalAfterInsurance = 0;

            // Xử lý khấu trừ tiền tạm ứng (Deposit)
            var deposits = await _context.Payments
                .Where(p => p.AppointmentId == appointmentId && p.IsDeposit)
                .ToListAsync();
            var totalDepositCollected = deposits.Sum(d => d.Amount);

            // Số tiền tạm ứng thực tế được áp dụng (không vượt quá tổng hóa đơn)
            var appliedDeposit = Math.Min(totalDepositCollected, subtotalAfterInsurance);

            if (appliedDeposit > 0)
            {
                var desc = totalDepositCollected > appliedDeposit
                    ? $"Tạm ứng đã thu (áp dụng {appliedDeposit:N0}/{totalDepositCollected:N0})"
                    : "Tạm ứng đã thu";

                lines.Add(new InvoiceLine
                {
                    Description = desc,
                    ItemType = "Deposit",
                    Amount = -appliedDeposit
                });
            }

            var balanceDue = subtotalAfterInsurance - appliedDeposit;
            if (balanceDue < 0) balanceDue = 0;

            // Xử lý tiền thừa tạm ứng (Refund) nếu có
            var depositRefund = totalDepositCollected - appliedDeposit;
            if (depositRefund > 0)
            {
                lines.Add(new InvoiceLine
                {
                    Description = "Hoàn tạm ứng dư",
                    ItemType = "DepositRefund",
                    Amount = -depositRefund
                });
                balanceDue = 0;
            }

            // Upsert hóa đơn phòng khám
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
                if (invoice.IsPaid) return invoice;

                invoice.Amount = subtotalAfterInsurance;
                invoice.BalanceDue = balanceDue;
                invoice.TotalDeposit = totalDepositCollected;

                // Làm mới danh sách chi tiết dòng hóa đơn
                _context.InvoiceLines.RemoveRange(invoice.InvoiceLines);
                invoice.InvoiceLines.Clear();
            }

            foreach (var line in lines)
            {
                line.InvoiceId = invoice.Id;
                _context.InvoiceLines.Add(line);
            }

            // Gắn InvoiceId cho các khoản tạm ứng để theo dõi vết thanh toán
            foreach (var deposit in deposits.Where(d => d.InvoiceId == null))
            {
                deposit.InvoiceId = invoice.Id;
            }

            await _context.SaveChangesAsync();
            return invoice;
        }
    }
}