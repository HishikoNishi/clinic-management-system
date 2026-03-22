using ClinicManagement.Api.Data;
using ClinicManagement.Api.Models;
using Microsoft.EntityFrameworkCore;
using ClinicManagement.Api.Dtos.MedicalRecords;
using System.Linq;

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

    public async Task<MedicalRecord> SubmitExaminationAsync(ExaminationRequestDto dto, Guid doctorUserId)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.UserId == doctorUserId);
            if (doctor == null) throw new InvalidOperationException("Doctor not found");

            var appointment = await _context.Appointments
                .Include(a => a.Patient)
                .FirstOrDefaultAsync(a => a.Id == dto.AppointmentId && a.DoctorId == doctor.Id);

            if (appointment == null) throw new InvalidOperationException("Appointment not found or not assigned to this doctor");

            if (appointment.Status == AppointmentStatus.Completed)
                throw new InvalidOperationException("Appointment already completed");

            var medicalRecord = new MedicalRecord
            {
                Id = Guid.NewGuid(),
                AppointmentId = appointment.Id,
                DoctorId = doctor.Id,
                PatientId = appointment.PatientId,
                Symptoms = appointment.Reason ?? string.Empty,
                Diagnosis = dto.Diagnosis,
                Treatment = string.Empty,
                Note = dto.Notes ?? string.Empty,
                CreatedAt = DateTime.UtcNow
            };

            _context.MedicalRecords.Add(medicalRecord);
            await _context.SaveChangesAsync();

            var validPrescriptionItems = dto.PrescriptionItems?
                .Where(item => !string.IsNullOrWhiteSpace(item.MedicineName))
                .ToList();

            if (validPrescriptionItems != null && validPrescriptionItems.Any())
            {
                var prescription = new Prescription
                {
                    Id = Guid.NewGuid(),
                    MedicalRecordId = medicalRecord.Id,
                    CreatedAt = DateTime.UtcNow,
                    PrescriptionDetails = validPrescriptionItems.Select(item => new PrescriptionDetail
                    {
                        Id = Guid.NewGuid(),
                        MedicineName = item.MedicineName,
                        Dosage = item.Dosage ?? string.Empty,
                        Duration = item.Quantity > 0 ? item.Quantity : 1,
                        Frequency = string.Empty
                    }).ToList()
                };

                _context.Prescriptions.Add(prescription);
            }

            if (dto.RequestClinicalTest)
            {
                _context.ClinicalTests.Add(new ClinicalTest
                {
                    MedicalRecordId = medicalRecord.Id,
                    TestName = string.IsNullOrWhiteSpace(dto.ClinicalTestName) ? "Clinical test requested" : dto.ClinicalTestName
                });
            }

            appointment.Status = AppointmentStatus.Completed;

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return medicalRecord;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<IEnumerable<BasicMedicalHistoryDto>> GetPatientRecordsForDoctor(Guid patientId, Guid doctorUserId)
    {
        var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.UserId == doctorUserId);
        if (doctor == null) throw new InvalidOperationException("Doctor not found");

        var hasRelation = await _context.Appointments.AnyAsync(a => a.PatientId == patientId && a.DoctorId == doctor.Id);
        if (!hasRelation) throw new InvalidOperationException("No permission to view this patient");

        var records = await _context.MedicalRecords
            .Where(r => r.PatientId == patientId && r.DoctorId == doctor.Id)
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new BasicMedicalHistoryDto
            {
                Id = r.Id,
                CreatedAt = r.CreatedAt,
                Diagnosis = r.Diagnosis,
                Note = r.Note
            })
            .ToListAsync();

        return records;
    }
}
