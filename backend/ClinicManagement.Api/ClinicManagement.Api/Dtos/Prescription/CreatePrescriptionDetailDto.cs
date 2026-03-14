namespace ClinicManagement.Api.Dtos.Prescription
{
    public class CreatePrescriptionDetailDto
    {
        public string MedicineName { get; set; }

        public string Dosage { get; set; }

        public string Frequency { get; set; }

        public int Duration { get; set; }
    }
}