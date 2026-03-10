using ClinicManagement.Api.Models;

public class PrescriptionDetail
{
    public Guid Id { get; set; }

    public Guid PrescriptionId { get; set; }

    public string MedicineName { get; set; }

    public string Dosage { get; set; }

    public string Frequency { get; set; }

    public int Duration { get; set; }

    public Prescription Prescription { get; set; }
}