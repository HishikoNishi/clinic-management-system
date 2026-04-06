using Microsoft.AspNetCore.Mvc;
using ClinicManagement.Api.Models;
using ClinicManagement.Api.Dtos.MedicalRecords;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using ClinicManagement.Api.Data;
using ClinicManagement.Api.Models;

[ApiController]
[Route("api/medical-record")]
[Authorize(Roles = "Doctor")]
public class MedicalRecordController : ControllerBase
{
    private readonly MedicalRecordService _service;
    private readonly ClinicDbContext _context;

    public MedicalRecordController(MedicalRecordService service, ClinicDbContext context)
    {
        _service = service;
        _context = context;
    }

    [HttpPost]
    public async Task<IActionResult> Create(MedicalRecord record)
    {
        var result = await _service.CreateMedicalRecord(record);
        return Ok(result);
    }

    [HttpPost("examination")]
    public async Task<IActionResult> SubmitExamination([FromBody] ExaminationRequestDto dto)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userIdClaim, out var doctorUserId))
            return Unauthorized("Invalid user id in token");

        try
        {
            var record = await _service.SubmitExaminationAsync(dto, doctorUserId);
            return Ok(record);
        }
        catch (InvalidOperationException ex)
        {
            // lỗi nghiệp vụ (không tìm thấy lịch / bác sĩ không sở hữu lịch / đã hoàn tất)
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            // fallback 500 kèm thông báo ngắn gọn cho client
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Could not submit examination", detail = ex.Message });
        }
    }

    [HttpGet("patient/{patientId:guid}")]
    public async Task<IActionResult> GetPatientRecords(Guid patientId)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userIdClaim, out var doctorUserId))
            return Unauthorized("Invalid user id in token");

        try
        {
            var records = await _service.GetPatientRecordsForDoctor(patientId, doctorUserId);
            return Ok(records);
        }
        catch (InvalidOperationException ex)
        {
            // Không có quyền hoặc không tìm thấy bác sĩ
            return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
        }
    }

    // Xem chi tiết hồ sơ cũ (bao gồm thuốc, xét nghiệm)
    [HttpGet("{recordId:guid}")]
    public async Task<IActionResult> GetRecordDetail(Guid recordId)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userIdClaim, out var doctorUserId))
            return Unauthorized("Invalid user id in token");

        var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.UserId == doctorUserId);
        if (doctor == null) return Unauthorized("Doctor not found");

        var record = await _context.MedicalRecords.FirstOrDefaultAsync(r => r.Id == recordId);
        if (record == null) return NotFound(new { message = "Medical record not found" });
        if (record.DoctorId != doctor.Id)
            return StatusCode(StatusCodes.Status403Forbidden, new { message = "Not allowed to view this record" });

        var prescription = await _context.Prescriptions
            .Include(p => p.PrescriptionDetails)
            .FirstOrDefaultAsync(p => p.MedicalRecordId == record.Id);

        var tests = await _context.ClinicalTests
            .Where(t => t.MedicalRecordId == record.Id)
            .Select(t => t.TestName)
            .ToListAsync();

        var dto = new ExaminationDetailDto
        {
            AppointmentId = record.AppointmentId,
            MedicalHistory = new(),
            Diagnosis = record.Diagnosis,
            Note = record.Note,
            InsuranceCoverPercent = record.InsuranceCoverPercent,
            Surcharge = record.Surcharge,
            Discount = record.Discount,
            PrescriptionItems = prescription?.PrescriptionDetails?.Select(d => new PrescriptionItemDto
            {
                MedicineName = d.MedicineName,
                Dosage = d.Dosage,
                Quantity = d.Duration
            }).ToList() ?? new List<PrescriptionItemDto>(),
            ClinicalTests = tests
        };

        return Ok(dto);
    }
}
