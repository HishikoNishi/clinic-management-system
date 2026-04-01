using ClinicManagement.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClinicManagement.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Cashier,Admin,Staff")]
    public class InsuranceController : ControllerBase
    {
        private readonly FakeInsuranceService _insuranceService;

        public InsuranceController(FakeInsuranceService insuranceService)
        {
            _insuranceService = insuranceService;
        }

        [HttpGet("validate")]
        public async Task<IActionResult> Validate([FromQuery] string code)
        {
            if (string.IsNullOrWhiteSpace(code))
                return BadRequest(new { message = "Thiếu mã bảo hiểm" });

            var plan = _insuranceService.Verify(code);
            if (plan == null)
                return NotFound(new { message = "Mã bảo hiểm không hợp lệ" });

            return Ok(new
            {
                plan.Code,
                plan.Name,
                plan.CoveragePercent,
                plan.ExpiryDate,
                plan.Note
            });
        }
    }
}
