using ClinicManagement.Api.Data;
using ClinicManagement.Api.Models;

public class PrescriptionService
{
    private readonly ClinicDbContext _context;

    public PrescriptionService(ClinicDbContext context)
    {
        _context = context;
    }

    public async Task<Prescription> CreatePrescription(Prescription prescription)
    {
        _context.Prescriptions.Add(prescription);
        await _context.SaveChangesAsync();
        return prescription;
    }
}