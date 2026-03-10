using ClinicManagement.Api.Data;
using ClinicManagement.Api.Models;
using Microsoft.EntityFrameworkCore;

public class MedicalRecordService
{
    private readonly ClinicDbContext _context;

    public MedicalRecordService(ClinicDbContext context)
    {
        _context = context;
    }

    public async Task<MedicalRecord> CreateMedicalRecord(MedicalRecord record)
    {
        _context.MedicalRecords.Add(record);

        var appointment = await _context.Appointments
     .FirstOrDefaultAsync(x => x.Id == record.AppointmentId);

        if (appointment != null)
        {
            appointment.Status = AppointmentStatus.Completed;
        }

        await _context.SaveChangesAsync();

        return record;
    }
}