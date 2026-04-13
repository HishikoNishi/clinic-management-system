using System;
using ClinicManagement.Api.Data;
using ClinicManagement.Api.Dtos.ClinicalTests;
using ClinicManagement.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Collections.Generic;

namespace ClinicManagement.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClinicalTestsController : ControllerBase
    {
        private readonly ClinicDbContext _context;

        public ClinicalTestsController(ClinicDbContext context)
        {
            _context = context;
        }

        // Doctor tạo yêu cầu xét nghiệm
        [HttpPost]
        public async Task<IActionResult> CreateTest(CreateClinicalTestDto dto)
        {
            var test = new ClinicalTest
            {
                MedicalRecordId = dto.MedicalRecordId,
                TestName = dto.TestName,
                Status = "Pending",
                OrderedByDoctorId = dto.OrderedByDoctorId
            };

            _context.ClinicalTests.Add(test);
            await _context.SaveChangesAsync();

            return Ok(ToDto(test));
        }

        // Technician nhập kết quả
        [HttpPatch("{id}/result")]
        public async Task<IActionResult> UpdateResult(int id, UpdateClinicalTestResultDto dto)
        {
            var test = await _context.ClinicalTests.FindAsync(id);

            if (test == null)
                return NotFound();

            test.Result = dto.Result;
            test.TechnicianName = dto.TechnicianName;
            test.Status = "Completed";
            test.ResultAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(ToDto(test));
        }

        // Technician bắt đầu làm
        [HttpPatch("{id}/start")]
        public async Task<IActionResult> StartTest(int id, [FromBody] StartClinicalTestDto dto)
        {
            var test = await _context.ClinicalTests.FindAsync(id);
            if (test == null) return NotFound();

            test.Status = "InProgress";
            if (!string.IsNullOrWhiteSpace(dto.TechnicianName))
                test.TechnicianName = dto.TechnicianName;

            await _context.SaveChangesAsync();
            return Ok(ToDto(test));
        }

        [HttpGet("medical-record/{medicalRecordId}")]
        public async Task<IActionResult> GetByMedicalRecord(Guid medicalRecordId)
        {
            var tests = await _context.ClinicalTests
                .AsNoTracking()
                .Where(t => t.MedicalRecordId == medicalRecordId)
                .OrderByDescending(t => t.CreatedAt)
                .Select(t => new ClinicalTestDto
                {
                    Id = t.Id,
                    MedicalRecordId = t.MedicalRecordId,
                    TestName = t.TestName,
                    Result = t.Result,
                    TechnicianName = t.TechnicianName,
                    CreatedAt = t.CreatedAt,
                    Status = string.IsNullOrWhiteSpace(t.Status)
                        ? (string.IsNullOrWhiteSpace(t.Result) ? "Pending" : "Completed")
                        : t.Status,
                    ResultAt = t.ResultAt,
                    AppointmentId = null,
                    AppointmentCode = null,
                    PatientId = null,
                    PatientName = null,
                    PatientPhone = null
                })
                .ToListAsync();

            return Ok(tests);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] string? status = null,
            [FromQuery] Guid? patientId = null,
            [FromQuery] bool paidOnly = false,
            [FromQuery] Guid? departmentId = null)
        {
            IQueryable<ClinicalTest> query = _context.ClinicalTests.AsNoTracking();

            // Apply filters before Include to avoid type conversion issues
            if (!string.IsNullOrWhiteSpace(status))
            {
                var statuses = status.Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => s.Trim().ToLower())
                    .ToList();
                if (statuses.Count > 0)
                    query = query.Where(t => statuses.Contains((t.Status ?? "Pending").ToLower()));
            }

            if (patientId.HasValue)
            {
                query = query.Where(t => t.MedicalRecord.PatientId == patientId.Value);
            }

            if (departmentId.HasValue)
            {
                var deptId = departmentId.Value;
                query = query.Where(t =>
                    _context.Doctors.Any(d => d.Id == t.MedicalRecord.DoctorId && d.DepartmentId == deptId));
            }

            if (paidOnly)
            {
                query = query.Where(t =>
                    _context.Invoices.Any(i => i.AppointmentId == t.MedicalRecord.AppointmentId &&
                                               (i.IsPaid || i.BalanceDue <= 0)));
            }

            // Apply Include after filters
            query = query.Include(t => t.MedicalRecord);

            var tests = await query
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();

            // Load related appointment and patient data
            var appointmentIds = tests.Select(t => t.MedicalRecord?.AppointmentId).Where(a => a.HasValue).Select(a => a.Value).Distinct().ToList();
            var appointments = appointmentIds.Any() 
                ? await _context.Appointments
                    .AsNoTracking()
                    .Include(a => a.Patient)
                    .Where(a => appointmentIds.Contains(a.Id))
                    .ToListAsync()
                : new List<Appointment>();

            var appointmentMap = appointments.ToDictionary(a => a.Id);

            var result = tests.Select(t => new ClinicalTestDto
            {
                Id = t.Id,
                MedicalRecordId = t.MedicalRecordId,
                TestName = t.TestName,
                Result = t.Result,
                TechnicianName = t.TechnicianName,
                CreatedAt = t.CreatedAt,
                Status = string.IsNullOrWhiteSpace(t.Status)
                    ? (string.IsNullOrWhiteSpace(t.Result) ? "Pending" : "Completed")
                    : t.Status,
                ResultAt = t.ResultAt,
                AppointmentId = t.MedicalRecord != null ? t.MedicalRecord.AppointmentId : null,
                AppointmentCode = t.MedicalRecord != null && appointmentMap.TryGetValue(t.MedicalRecord.AppointmentId, out var apt0)
                    ? apt0.AppointmentCode
                    : null,
                PatientId = t.MedicalRecord != null ? t.MedicalRecord.PatientId : null,
                PatientName = t.MedicalRecord != null && appointmentMap.TryGetValue(t.MedicalRecord.AppointmentId, out var apt) 
                    ? apt.Patient?.FullName 
                    : null,
                PatientPhone = t.MedicalRecord != null && appointmentMap.TryGetValue(t.MedicalRecord.AppointmentId, out var apt2) 
                    ? apt2.Patient?.Phone 
                    : null,
                PatientCode = t.MedicalRecord != null && appointmentMap.TryGetValue(t.MedicalRecord.AppointmentId, out var apt3)
        ? apt3.Patient?.PatientCode 
        : null,
                CitizenId = t.MedicalRecord != null && appointmentMap.TryGetValue(t.MedicalRecord.AppointmentId, out var apt4)
        ? apt4.Patient?.CitizenId 
        : null,
                InsuranceCardNumber = t.MedicalRecord != null && appointmentMap.TryGetValue(t.MedicalRecord.AppointmentId, out var apt5)
        ? apt5.Patient?.InsuranceCardNumber 
        : null
            }).ToList();

            return Ok(result);
        }

        // Danh sách bệnh nhân có xét nghiệm cần làm (đã thanh toán)
        [HttpGet("pending-patients")]
        public async Task<IActionResult> GetPendingPatients()
        {
            return await GetPendingPatientsInternal(null, true);
        }

        [HttpGet("pending-patients/by-department")]
        public async Task<IActionResult> GetPendingPatientsByDepartment([FromQuery] Guid departmentId, [FromQuery] bool paidOnly = true)
        {
            return await GetPendingPatientsInternal(departmentId, paidOnly);
        }

        private async Task<IActionResult> GetPendingPatientsInternal(Guid? departmentId, bool paidOnly)
        {
            var query = _context.ClinicalTests
                .AsNoTracking()
                .Where(t => t.Status != "Completed")
                .Include(t => t.MedicalRecord)
                .AsQueryable();

            if (departmentId.HasValue)
            {
                var deptId = departmentId.Value;
                query = query.Where(t =>
                    _context.Doctors.Any(d => d.Id == t.MedicalRecord.DoctorId && d.DepartmentId == deptId));
            }

            if (paidOnly)
            {
                query = query.Where(t =>
                    _context.Invoices.Any(i => i.AppointmentId == t.MedicalRecord.AppointmentId &&
                                               (i.IsPaid || i.BalanceDue <= 0)));
            }

            var tests = await query.ToListAsync();

            // Load related appointment and patient data
            var appointmentIds = tests.Select(t => t.MedicalRecord?.AppointmentId).Where(a => a.HasValue).Select(a => a.Value).Distinct().ToList();
            var appointments = await _context.Appointments
                .AsNoTracking()
                .Include(a => a.Patient)
                .Where(a => appointmentIds.Contains(a.Id))
                .ToListAsync();

            var appointmentMap = appointments.ToDictionary(a => a.Id);

            var patients = tests
                .GroupBy(t => new { t.MedicalRecord.PatientId, t.MedicalRecordId, t.MedicalRecord.AppointmentId })
                .Select(g => new PatientTestSummaryDto
                {
                    PatientId = g.Key.PatientId,
                    FullName = appointmentMap.TryGetValue(g.Key.AppointmentId, out var apt) 
                        ? apt.Patient?.FullName ?? string.Empty 
                        : string.Empty,
                    Phone = appointmentMap.TryGetValue(g.Key.AppointmentId, out var apt2) 
                        ? apt2.Patient?.Phone 
                        : null,
                    AppointmentCode = appointmentMap.TryGetValue(g.Key.AppointmentId, out var apt3)
                        ? apt3.AppointmentCode
                        : null,
                    PendingCount = g.Count(),
                    AppointmentId = g.Key.AppointmentId,
                    MedicalRecordId = g.Key.MedicalRecordId,
                    PatientCode = appointmentMap.TryGetValue(g.Key.AppointmentId, out var apt4) ? apt4.Patient?.PatientCode : null,
                    CitizenId = appointmentMap.TryGetValue(g.Key.AppointmentId, out var apt5) ? apt5.Patient?.CitizenId : null,
                    InsuranceCardNumber = appointmentMap.TryGetValue(g.Key.AppointmentId, out var apt6) ? apt6.Patient?.InsuranceCardNumber : null,
                })
                .OrderByDescending(x => x.PendingCount)
                .ToList();

            return Ok(patients);
        }

        private static ClinicalTestDto ToDto(ClinicalTest test) => new ClinicalTestDto
        {
            Id = test.Id,
            MedicalRecordId = test.MedicalRecordId,
            TestName = test.TestName,
            Result = test.Result,
            TechnicianName = test.TechnicianName,
            CreatedAt = test.CreatedAt,
            Status = string.IsNullOrWhiteSpace(test.Status)
                ? (string.IsNullOrWhiteSpace(test.Result) ? "Pending" : "Completed")
                : test.Status,
            ResultAt = test.ResultAt,
            AppointmentId = null,
            AppointmentCode = null,
            PatientId = null,
            PatientName = null,
            PatientPhone = null
        };
    }
}
