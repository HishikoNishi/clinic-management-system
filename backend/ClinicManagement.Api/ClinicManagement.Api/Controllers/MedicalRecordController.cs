using Microsoft.AspNetCore.Mvc;
using ClinicManagement.Api.Models;

[ApiController]
[Route("api/medical-record")]
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
}