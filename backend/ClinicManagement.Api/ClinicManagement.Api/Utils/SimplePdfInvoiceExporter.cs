using ClinicManagement.Api.Models;
using System.Globalization;
using System.Text;

namespace ClinicManagement.Api.Utils
{
    public static class SimplePdfInvoiceExporter
    {
        public static byte[] Build(Invoice invoice, MedicalRecord? record)
        {
            var lines = BuildLines(invoice, record);
            return BuildPdf(lines);
        }

        private static List<string> BuildLines(Invoice invoice, MedicalRecord? record)
        {
            var lines = new List<string>
            {
                "CLINIC INVOICE",
                $"Invoice ID: {invoice.Id}",
                $"Appointment: {invoice.Appointment?.AppointmentCode ?? invoice.AppointmentId.ToString()}",
                $"Patient: {invoice.Appointment?.Patient?.FullName ?? "N/A"}",
                $"Date: {invoice.CreatedAt:yyyy-MM-dd HH:mm:ss}",
                $"Payment Date: {(invoice.PaymentDate.HasValue ? invoice.PaymentDate.Value.ToString("yyyy-MM-dd HH:mm:ss") : "N/A")}",
                $"Type: {invoice.InvoiceType}",
                $"Insurance Cover: {Math.Round((record?.InsuranceCoverPercent ?? 0m) * 100m)}%",
                "----------------------------------------",
                "DETAILS:"
            };

            foreach (var line in invoice.InvoiceLines ?? new List<InvoiceLine>())
            {
                lines.Add($"{line.Description} | {line.ItemType} | {line.Amount.ToString("N0", CultureInfo.InvariantCulture)} VND");
            }

            lines.Add("----------------------------------------");
            lines.Add($"Amount: {invoice.Amount.ToString("N0", CultureInfo.InvariantCulture)} VND");
            lines.Add($"Deposit: {invoice.TotalDeposit.ToString("N0", CultureInfo.InvariantCulture)} VND");
            lines.Add($"Balance Due: {invoice.BalanceDue.ToString("N0", CultureInfo.InvariantCulture)} VND");
            lines.Add($"Paid: {(invoice.IsPaid ? "YES" : "NO")}");
            lines.Add("Thank you.");
            return lines;
        }

        private static byte[] BuildPdf(IReadOnlyList<string> lines)
        {
            var textBuilder = new StringBuilder();
            textBuilder.AppendLine("BT");
            textBuilder.AppendLine("/F1 11 Tf");
            textBuilder.AppendLine("50 800 Td");

            foreach (var line in lines)
            {
                textBuilder.AppendLine($"({EscapeText(ToAscii(line))}) Tj");
                textBuilder.AppendLine("0 -15 Td");
            }

            textBuilder.AppendLine("ET");
            var textContent = textBuilder.ToString();
            var textBytes = Encoding.ASCII.GetBytes(textContent);

            var objects = new List<string>
            {
                "1 0 obj << /Type /Catalog /Pages 2 0 R >> endobj",
                "2 0 obj << /Type /Pages /Kids [3 0 R] /Count 1 >> endobj",
                "3 0 obj << /Type /Page /Parent 2 0 R /MediaBox [0 0 595 842] /Resources << /Font << /F1 4 0 R >> >> /Contents 5 0 R >> endobj",
                "4 0 obj << /Type /Font /Subtype /Type1 /BaseFont /Helvetica >> endobj",
                $"5 0 obj << /Length {textBytes.Length} >> stream\n{textContent}\nendstream endobj"
            };

            var pdf = new StringBuilder();
            pdf.AppendLine("%PDF-1.4");

            var offsets = new List<int>();
            foreach (var obj in objects)
            {
                offsets.Add(Encoding.ASCII.GetByteCount(pdf.ToString()));
                pdf.AppendLine(obj);
            }

            var xrefOffset = Encoding.ASCII.GetByteCount(pdf.ToString());
            pdf.AppendLine("xref");
            pdf.AppendLine($"0 {objects.Count + 1}");
            pdf.AppendLine("0000000000 65535 f ");
            foreach (var offset in offsets)
            {
                pdf.AppendLine($"{offset:D10} 00000 n ");
            }

            pdf.AppendLine("trailer");
            pdf.AppendLine($"<< /Size {objects.Count + 1} /Root 1 0 R >>");
            pdf.AppendLine("startxref");
            pdf.AppendLine(xrefOffset.ToString());
            pdf.AppendLine("%%EOF");

            return Encoding.ASCII.GetBytes(pdf.ToString());
        }

        private static string EscapeText(string value)
            => value.Replace("\\", "\\\\").Replace("(", "\\(").Replace(")", "\\)");

        private static string ToAscii(string input)
        {
            var normalized = input.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder(normalized.Length);
            foreach (var ch in normalized)
            {
                var unicode = CharUnicodeInfo.GetUnicodeCategory(ch);
                if (unicode == UnicodeCategory.NonSpacingMark) continue;
                sb.Append(ch <= 127 ? ch : '?');
            }

            return sb.ToString().Normalize(NormalizationForm.FormC);
        }
    }
}

