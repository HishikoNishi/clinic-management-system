using ClinicManagement.Api.Dtos.Otp;
using ClinicManagement.Api.Extensions;
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
            await _otpService.SendOtpAsync(dto.Email);
            return Ok(new { message = "OTP đã được gửi" });
        }

        [HttpPost("verify-otp")]
        public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpRequestDto dto)
        {
            var ok = await _otpService.VerifyOtpAsync(dto.Email, dto.Code);
            if (!ok) return this.ApiBadRequest("OTP sai hoặc đã hết hạn", "otp_invalid");

            return Ok(new { message = "Xác thực OTP thành công" });
        }
    }
}
