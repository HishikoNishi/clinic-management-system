namespace ClinicManagement.Api.Models
{
    public class InvoiceLine
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid InvoiceId { get; set; }
        public Invoice? Invoice { get; set; }

        public string Description { get; set; } = string.Empty;

        // Kiểu dòng: Consultation | Drug | Test | Surcharge | Discount | Insurance
        public string ItemType { get; set; } = "Consultation";

        public decimal Amount { get; set; }
        // Optional fields (mainly for Drug lines). Keep non-null to avoid DB NOT NULL issues.
        public string Dosage { get; set; } = string.Empty;
        public int Duration { get; set; } = 0;
    }
}
