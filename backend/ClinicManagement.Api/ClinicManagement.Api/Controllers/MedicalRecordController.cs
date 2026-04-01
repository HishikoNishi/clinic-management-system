using Microsoft.AspNetCore.Mvc;
using ClinicManagement.Api.Models;
using ClinicManagement.Api.Dtos.MedicalRecords;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

[ApiController]
[Route("api/medical-record")]
[Authorize(Roles = "Doctor")]
public class MedicalRecordController : ControllerBase
{
    private readonly MedicalRecordService _service;

    public MedicalRecordController(MedicalRecordService service)
    {
        _service = service;
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
}
