namespace ClinicManagement.Api.Dtos.MedicalRecords
{
    public class PrescriptionItemDto
    {
        public string MedicineName { get; set; } = string.Empty;
        public string Dosage { get; set; } = string.Empty;
        public int Quantity { get; set; }
    }
}
