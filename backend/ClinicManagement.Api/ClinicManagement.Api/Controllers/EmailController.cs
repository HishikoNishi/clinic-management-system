using ClinicManagement.Api.Dtos.Otp;
using ClinicManagement.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace ClinicManagement.Api.Controllers
{
    [ApiController]
    [Route("api/email")]
    public class EmailController : ControllerBase
    {
        private readonly OtpService _otpService;

        public EmailController(OtpService otpService)
        {
            _otpService = otpService;
        }

        [HttpPost("send-otp")]
        public async Task<IActionResult> SendOtp([FromBody] SendOtpRequestDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            await _otpService.SendOtpAsync(dto.Email);
            return Ok(new { message = "OTP đã được gửi" });
        }

        [HttpPost("verify-otp")]
        public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpRequestDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var ok = await _otpService.VerifyOtpAsync(dto.Email, dto.Code);
            if (!ok) return BadRequest(new { message = "OTP sai hoặc đã hết hạn" });

            return Ok(new { message = "Xác thực OTP thành công" });
        }
    }
}
