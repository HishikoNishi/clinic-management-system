using Microsoft.AspNetCore.Mvc;
using ClinicManagement.Api.Models;

[ApiController]
[Route("api/prescription")]
public class PrescriptionController : ControllerBase
{
    private readonly PrescriptionService _service;

    public PrescriptionController(PrescriptionService service)
    {
        _service = service;
    }

    [HttpPost]
    public async Task<IActionResult> Create(Prescription prescription)
    {
        var result = await _service.CreatePrescription(prescription);
        return Ok(result);
    }
}