using ClinicManagement.Api.Data;
using ClinicManagement.Api.Models;
using Microsoft.EntityFrameworkCore;
using ClinicManagement.Api.Dtos.MedicalRecords;
using System.Linq;
using ClinicManagement.Api.Services;

public class MedicalRecordService
{
    private readonly ClinicDbContext _context;
    private readonly BillingService _billingService;

    public MedicalRecordService(ClinicDbContext context, BillingService billingService)
    {
        _context = context;
        _billingService = billingService;
    }

    public async Task<MedicalRecord> CreateMedicalRecord(MedicalRecord record)
    {
        _context.MedicalRecords.Add(record);
        var appointment = await _context.Appointments.FirstOrDefaultAsync(x => x.Id == record.AppointmentId);
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

            // allow update if record exists
            var medicalRecord = await _context.MedicalRecords
                .FirstOrDefaultAsync(r => r.AppointmentId == appointment.Id);

            if (medicalRecord == null)
            {
                medicalRecord = new MedicalRecord
                {
                    Id = Guid.NewGuid(),
                    AppointmentId = appointment.Id,
                    DoctorId = doctor.Id,
                    PatientId = appointment.PatientId,
                    Symptoms = appointment.Reason ?? string.Empty,
                    CreatedAt = DateTime.UtcNow
                };
                _context.MedicalRecords.Add(medicalRecord);
            }

            // Ưu tiên giá trị đã lưu (ví dụ đã xác thực BHYT khi check-in) nếu payload không gửi hoặc =0
            var insurance = dto.InsuranceCoverPercent;
            if (insurance <= 0 && medicalRecord.InsuranceCoverPercent > 0)
            {
                insurance = medicalRecord.InsuranceCoverPercent;
            }
            insurance = FinanceHelper.Clamp01(insurance);

            medicalRecord.Diagnosis = dto.Diagnosis;
            // EF column Treatment không nullable, ghi cùng chẩn đoán để tránh null
            medicalRecord.Treatment = dto.Diagnosis;
            medicalRecord.Note = dto.Notes ?? string.Empty;
            medicalRecord.InsuranceCoverPercent = insurance;
            medicalRecord.Surcharge = dto.Surcharge;
            medicalRecord.Discount = dto.Discount;

            // Replace prescription
            var existingPrescription = await _context.Prescriptions
                .Include(p => p.PrescriptionDetails)
                .FirstOrDefaultAsync(p => p.MedicalRecordId == medicalRecord.Id);

            if (existingPrescription != null)
            {
                if (existingPrescription.PrescriptionDetails != null)
                {
                    _context.PrescriptionDetails.RemoveRange(existingPrescription.PrescriptionDetails);
                }
                _context.Prescriptions.Remove(existingPrescription);
            }

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

            // Replace clinical tests (tránh cộng dồn qua nhiều lần lưu)
            var existingTests = await _context.ClinicalTests
                .Where(t => t.MedicalRecordId == medicalRecord.Id)
                .ToListAsync();
            if (existingTests.Any())
            {
                _context.ClinicalTests.RemoveRange(existingTests);
            }

            var testNames = new List<string>();
            if (dto.ClinicalTestNames != null && dto.ClinicalTestNames.Any())
            {
                testNames.AddRange(dto.ClinicalTestNames.Where(n => !string.IsNullOrWhiteSpace(n)));
            }
            else if (dto.RequestClinicalTest && !string.IsNullOrWhiteSpace(dto.ClinicalTestName))
            {
                testNames.Add(dto.ClinicalTestName);
            }

            foreach (var testName in testNames)
            {
                _context.ClinicalTests.Add(new ClinicalTest
                {
                    MedicalRecordId = medicalRecord.Id,
                    TestName = testName,
                    Status = "Pending",
                    OrderedByDoctorId = doctor.Id
                });
            }

            appointment.Status = AppointmentStatus.Completed;

            await _context.SaveChangesAsync();

            try
            {
                await _billingService.GenerateInvoiceAsync(appointment.Id);
            }
            catch
            {
                // Nếu tính hóa đơn lỗi (thiếu cấu hình giá, v.v.), vẫn giữ hồ sơ khám
                // và để cashier tính lại sau.
            }
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
