using ClinicManagement.Api.Data;
using ClinicManagement.Api.Models;
using Microsoft.EntityFrameworkCore;
using ClinicManagement.Api.Dtos.MedicalRecords;
using System.Linq;
using ClinicManagement.Api.Services;
using System.Text.RegularExpressions;

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

            ValidateClinicalMeasures(dto);

            var insurance = dto.InsuranceCoverPercent;
            if (insurance <= 0 && medicalRecord.InsuranceCoverPercent > 0)
            {
                insurance = medicalRecord.InsuranceCoverPercent;
            }
            insurance = FinanceHelper.Clamp01(insurance);

            medicalRecord.Symptoms = NormalizeText(dto.ChiefComplaint) ?? appointment.Reason ?? medicalRecord.Symptoms ?? string.Empty;
            medicalRecord.DetailedSymptoms = NormalizeText(dto.DetailedSymptoms) ?? medicalRecord.DetailedSymptoms;
            medicalRecord.PastMedicalHistory = NormalizeText(dto.PastMedicalHistory) ?? medicalRecord.PastMedicalHistory;
            medicalRecord.Allergies = NormalizeText(dto.Allergies) ?? medicalRecord.Allergies;
            medicalRecord.Occupation = NormalizeText(dto.Occupation) ?? medicalRecord.Occupation;
            medicalRecord.Habits = NormalizeText(dto.Habits) ?? medicalRecord.Habits;
            medicalRecord.HeightCm = dto.HeightCm ?? medicalRecord.HeightCm;
            medicalRecord.WeightKg = dto.WeightKg ?? medicalRecord.WeightKg;
            medicalRecord.Bmi = CalculateBmi(medicalRecord.WeightKg, medicalRecord.HeightCm);
            medicalRecord.HeartRate = dto.HeartRate ?? medicalRecord.HeartRate;
            medicalRecord.BloodPressure = NormalizeBloodPressure(dto.BloodPressure) ?? medicalRecord.BloodPressure;
            medicalRecord.Temperature = dto.Temperature ?? medicalRecord.Temperature;
            medicalRecord.Spo2 = dto.Spo2 ?? medicalRecord.Spo2;
            medicalRecord.Diagnosis = dto.Diagnosis;
            medicalRecord.Treatment = dto.Diagnosis;
            medicalRecord.Note = dto.Notes ?? string.Empty;
            medicalRecord.InsuranceCoverPercent = insurance;
            medicalRecord.Surcharge = dto.Surcharge;
            medicalRecord.Discount = dto.Discount;

            // Replace prescription
            var existingPrescription = await _context.Prescriptions
                .Include(p => p.PrescriptionDetails)
                .FirstOrDefaultAsync(p => p.MedicalRecordId == medicalRecord.Id);

            var validPrescriptionItems = dto.PrescriptionItems?
                .Where(item => !string.IsNullOrWhiteSpace(item.MedicineName))
                .ToList();

            if (validPrescriptionItems != null && validPrescriptionItems.Any())
            {
                if (existingPrescription != null)
                {
                    if (existingPrescription.PrescriptionDetails != null)
                    {
                        _context.PrescriptionDetails.RemoveRange(existingPrescription.PrescriptionDetails);
                    }
                    _context.Prescriptions.Remove(existingPrescription);
                }

                // 2. Tạo đơn mới
                var prescription = new Prescription
                {
                    Id = Guid.NewGuid(),
                    MedicalRecordId = medicalRecord.Id,
                    CreatedAt = DateTime.UtcNow
                };

                var details = new List<PrescriptionDetail>();

                foreach (var item in validPrescriptionItems)
                {
                    // Only search by MedicineName since PrescriptionItemDto does not have MedicineId
                    var searchName = item.MedicineName?.Trim();

                    var medicine = await _context.Medicines
                        .AsNoTracking()
                        .FirstOrDefaultAsync(m => m.Name == searchName);

                    details.Add(new PrescriptionDetail
                    {
                        Id = Guid.NewGuid(),
                        PrescriptionId = prescription.Id,
                        // MedicineId is set from found medicine, or null if not found
                        MedicineId = medicine?.Id,
                        MedicineName = item.MedicineName,
                        Dosage = item.Dosage ?? string.Empty,
                        Duration = item.Quantity > 0 ? item.Quantity : 1,
                        TotalQuantity = item.Quantity > 0 ? item.Quantity : 1,
                        UnitPrice = medicine?.Price ?? 0,
                        Frequency = string.Empty
                    });
                }

                prescription.PrescriptionDetails = details;
                _context.Prescriptions.Add(prescription);
            }

            // ... (đoạn code phía dưới giữ nguyên)
            // Replace clinical tests (tránh cộng dồn qua nhiều lần lưu)
            var existingTests = await _context.ClinicalTests
                .Where(t => t.MedicalRecordId == medicalRecord.Id)
                .ToListAsync();

            var testNames = new List<string>();
            if (dto.ClinicalTestNames != null && dto.ClinicalTestNames.Any())
            {
                testNames.AddRange(dto.ClinicalTestNames.Where(n => !string.IsNullOrWhiteSpace(n)));
            }
            else if (dto.RequestClinicalTest && !string.IsNullOrWhiteSpace(dto.ClinicalTestName))
            {
                testNames.Add(dto.ClinicalTestName);
            }

            foreach (var testName in testNames.Distinct(StringComparer.OrdinalIgnoreCase))
            {
                var exists = existingTests.Any(t => string.Equals(t.TestName, testName, StringComparison.OrdinalIgnoreCase));
                if (exists) continue;

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

    private static string? NormalizeText(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private static decimal? CalculateBmi(decimal? weightKg, decimal? heightCm)
    {
        if (!weightKg.HasValue || !heightCm.HasValue) return null;
        if (weightKg <= 0 || heightCm <= 0) return null;

        var heightM = heightCm.Value / 100m;
        if (heightM <= 0) return null;

        return Math.Round(weightKg.Value / (heightM * heightM), 1);
    }

    private static string? NormalizeBloodPressure(string? bloodPressure)
    {
        if (string.IsNullOrWhiteSpace(bloodPressure)) return null;
        var match = Regex.Match(bloodPressure.Trim(), @"^(\d{2,3})\s*/\s*(\d{2,3})$");
        return match.Success ? $"{match.Groups[1].Value}/{match.Groups[2].Value}" : null;
    }

    private static (int Systolic, int Diastolic)? ParseBloodPressure(string? bloodPressure)
    {
        if (string.IsNullOrWhiteSpace(bloodPressure)) return null;
        var match = Regex.Match(bloodPressure.Trim(), @"^(\d{2,3})\s*/\s*(\d{2,3})$");
        if (!match.Success) return null;
        return (int.Parse(match.Groups[1].Value), int.Parse(match.Groups[2].Value));
    }

    private static void ValidateClinicalMeasures(ExaminationRequestDto dto)
    {
        if (dto.HeightCm.HasValue && (dto.HeightCm < 30m || dto.HeightCm > 250m))
            throw new InvalidOperationException("Chiều cao phải trong khoảng 30-250 cm");

        if (dto.WeightKg.HasValue && (dto.WeightKg < 1m || dto.WeightKg > 400m))
            throw new InvalidOperationException("Cân nặng phải trong khoảng 1-400 kg");

        if (dto.HeartRate.HasValue && (dto.HeartRate < 30 || dto.HeartRate > 220))
            throw new InvalidOperationException("Nhịp tim phải trong khoảng 30-220 bpm");

        if (dto.Temperature.HasValue && (dto.Temperature < 34m || dto.Temperature > 42m))
            throw new InvalidOperationException("Nhiệt độ phải trong khoảng 34-42 °C");

        if (dto.Spo2.HasValue && (dto.Spo2 < 70 || dto.Spo2 > 100))
            throw new InvalidOperationException("SpO2 phải trong khoảng 70-100%");

        var parsedBp = ParseBloodPressure(dto.BloodPressure);
        if (!string.IsNullOrWhiteSpace(dto.BloodPressure) && parsedBp == null)
            throw new InvalidOperationException("Huyết áp phải theo dạng 120/80");

        if (parsedBp.HasValue)
        {
            var (systolic, diastolic) = parsedBp.Value;
            if (systolic < 70 || systolic > 250 || diastolic < 40 || diastolic > 150 || systolic <= diastolic)
                throw new InvalidOperationException("Huyết áp vượt giới hạn hợp lệ");
        }
    }
}
