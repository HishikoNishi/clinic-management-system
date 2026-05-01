using ClinicManagement.Api.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Globalization;

namespace ClinicManagement.Api.Utils
{
    public static class InvoicePdfExporter
    {
        static InvoicePdfExporter()
        {
            // Bắt buộc khai báo license trước khi dùng QuestPDF
            // Community license miễn phí cho dự án có doanh thu < $1M/năm
            QuestPDF.Settings.License = LicenseType.Community;
        }

        public static byte[] Build(Invoice invoice, MedicalRecord? record)
        {
            return Document.Create(container => container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(36);
                page.DefaultTextStyle(x => x.FontFamily("Arial").FontSize(10));

                page.Header().Element(header => ComposeHeader(header, invoice));
                page.Content().Element(content => ComposeContent(content, invoice, record));
                page.Footer().Element(ComposeFooter);
            }))
            .GeneratePdf();
        }

        // =============================================
        // HEADER
        // =============================================
        private static void ComposeHeader(IContainer container, Invoice invoice)
        {
            container
                .Background("#F0F5FF")
                .Padding(12)
                .Row(row =>
                {
                    row.RelativeItem().Column(col =>
                    {
                        col.Item().Text("CLINIC MANAGEMENT SYSTEM")
                            .FontSize(16).Bold().FontColor("#1A3C6E");
                        col.Item().Text("HÓA ĐƠN THANH TOÁN")
                            .FontSize(10).FontColor("#555555");
                    });

                    row.ConstantItem(180).Column(col =>
                    {
                        col.Item().AlignRight().Text($"Mã HĐ: {invoice.Id.ToString("N")[..10].ToUpperInvariant()}")
                            .FontSize(9).FontColor("#555555");
                        col.Item().AlignRight()
                            .Text(invoice.IsPaid ? "ĐÃ THANH TOÁN" : "CHƯA THANH TOÁN")
                            .FontSize(11).Bold()
                            .FontColor(invoice.IsPaid ? "#1A7A4A" : "#C0392B");
                    });
                });
        }

        // =============================================
        // CONTENT
        // =============================================
        private static void ComposeContent(IContainer container, Invoice invoice, MedicalRecord? record)
        {
            var vi = new CultureInfo("vi-VN");
            var appointmentCode = invoice.Appointment?.AppointmentCode
                                  ?? invoice.AppointmentId.ToString("N")[..8];
            var patientName = invoice.Appointment?.Patient?.FullName ?? "N/A";
            var patientPhone = invoice.Appointment?.Patient?.Phone ?? "N/A";
            var insuranceCover = Math.Round((record?.InsuranceCoverPercent ?? 0m) * 100m);

            container.Column(col =>
            {
                col.Spacing(12);

                // ---- Info boxes ----
                col.Item().Row(row =>
                {
                    row.RelativeItem().Border(1).BorderColor("#CCCCCC").Padding(10).Column(c =>
                    {
                        c.Item().Text("THÔNG TIN BỆNH NHÂN").Bold().FontSize(9).FontColor("#555555");
                        c.Item().Height(6);
                        InfoLine(c, "Họ tên", patientName);
                        InfoLine(c, "Điện thoại", patientPhone);
                        InfoLine(c, "Mã lịch hẹn", appointmentCode);
                        InfoLine(c, "Bảo hiểm", $"{insuranceCover}%");
                    });

                    row.ConstantItem(12); // gap

                    row.RelativeItem().Border(1).BorderColor("#CCCCCC").Padding(10).Column(c =>
                    {
                        c.Item().Text("THÔNG TIN HÓA ĐƠN").Bold().FontSize(9).FontColor("#555555");
                        c.Item().Height(6);
                        InfoLine(c, "Loại HĐ", invoice.InvoiceType.ToString());
                        InfoLine(c, "Ngày tạo", invoice.CreatedAt.ToString("dd/MM/yyyy HH:mm"));
                        InfoLine(c, "Ngày thanh toán",
                            invoice.PaymentDate.HasValue
                                ? invoice.PaymentDate.Value.ToString("dd/MM/yyyy HH:mm")
                                : "N/A");
                        InfoLine(c, "Mã tham chiếu", invoice.AppointmentId.ToString("N")[..10].ToUpperInvariant());
                    });
                });

                // ---- Line items table ----
                var lines = invoice.InvoiceLines ?? new List<InvoiceLine>();

                col.Item().Table(table =>
                {
                    table.ColumnsDefinition(cols =>
                    {
                        cols.RelativeColumn(5);   // Mô tả
                        cols.RelativeColumn(2);   // Loại
                        cols.RelativeColumn(2);   // Số tiền
                    });

                    // Header row
                    table.Header(header =>
                    {
                        header.Cell().Background("#1A3C6E").Padding(6)
                            .Text("Mô tả").Bold().FontColor(Colors.White).FontSize(10);
                        header.Cell().Background("#1A3C6E").Padding(6)
                            .Text("Loại").Bold().FontColor(Colors.White).FontSize(10);
                        header.Cell().Background("#1A3C6E").Padding(6).AlignRight()
                            .Text("Số tiền (VNĐ)").Bold().FontColor(Colors.White).FontSize(10);
                    });

                    if (lines.Count == 0)
                    {
                        table.Cell().ColumnSpan(3).Padding(8)
                            .Text("Không có dòng chi tiết.").Italic().FontColor("#888888");
                    }
                    else
                    {
                        var isOdd = false;
                        foreach (var line in lines)
                        {
                            var bg = isOdd ? "#F7F9FC" : "#FFFFFF";
                            isOdd = !isOdd;

                            table.Cell().Background(bg).Padding(6).Text(line.Description ?? "N/A");
                            table.Cell().Background(bg).Padding(6).Text(line.ItemType ?? "N/A");
                            table.Cell().Background(bg).Padding(6).AlignRight()
                                .Text(line.Amount.ToString("N0", vi));
                        }
                    }
                });

                // ---- Totals box ----
                col.Item().AlignRight().Width(260).Border(1).BorderColor("#CCCCCC").Padding(12).Column(c =>
                {
                    c.Spacing(4);
                    TotalLine(c, "Tạm tính", invoice.Amount.ToString("N0", vi) + " VNĐ");
                    TotalLine(c, "Đặt cọc", invoice.TotalDeposit.ToString("N0", vi) + " VNĐ");
                    TotalLine(c, "Còn lại", invoice.BalanceDue.ToString("N0", vi) + " VNĐ");
                    TotalLine(c, "Bảo hiểm", $"{insuranceCover}%");

                    c.Item().PaddingTop(6)
                        .Text(invoice.IsPaid ? "✓ Đã thanh toán" : "⚠ Chờ thanh toán")
                        .Bold()
                        .FontColor(invoice.IsPaid ? "#1A7A4A" : "#C0392B");
                });
            });
        }

        // =============================================
        // FOOTER
        // =============================================
        private static void ComposeFooter(IContainer container)
        {
            container
                .BorderTop(1).BorderColor("#DDDDDD")
                .PaddingTop(8)
                .Row(row =>
                {
                    row.RelativeItem().Text("Hệ thống Quản lý Phòng khám")
                        .FontSize(8).FontColor("#888888");
                    row.RelativeItem().AlignRight()
                        .Text($"In lúc: {DateTime.Now:dd/MM/yyyy HH:mm:ss}")
                        .FontSize(8).FontColor("#888888");
                });
        }

        // =============================================
        // HELPERS
        // =============================================
        private static void InfoLine(ColumnDescriptor col, string label, string value)
        {
            col.Item().Row(row =>
            {
                row.ConstantItem(110).Text(label + ":").FontColor("#555555");
                row.RelativeItem().Text(value).Bold();
            });
        }

        private static void TotalLine(ColumnDescriptor col, string label, string value)
        {
            col.Item().Row(row =>
            {
                row.RelativeItem().Text(label + ":").FontColor("#555555");
                row.ConstantItem(120).AlignRight().Text(value).Bold();
            });
        }
    }
}