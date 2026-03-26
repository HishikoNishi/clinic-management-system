using ClinicManagement.Api.Data;
using ClinicManagement.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ClinicManagement.Api.Services
{
    public class OtpService
    {
        private readonly ClinicDbContext _context;
        private readonly EmailService _emailService;
        private readonly ILogger<OtpService> _logger;
        private const int OtpLength = 6;
        private const int ExpireMinutes = 5;

        public OtpService(ClinicDbContext context, EmailService emailService, ILogger<OtpService> logger)
        {
            _context = context;
            _emailService = emailService;
            _logger = logger;
        }

        public async Task SendOtpAsync(string email)
        {
            var code = GenerateCode();
            var otp = new EmailOtp
            {
                Email = email,
                Code = code,
                ExpiredAt = DateTime.UtcNow.AddMinutes(ExpireMinutes),
                IsUsed = false
            };

            _context.EmailOtps.Add(otp);
            await _context.SaveChangesAsync();

            try
            {
                await _emailService.SendAsync(email, "Mã OTP xác thực lịch khám", $"Mã OTP của bạn: <b>{code}</b> (hiệu lực {ExpireMinutes} phút)");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Send OTP email failed");
                throw;
            }
        }

        public async Task<bool> VerifyOtpAsync(string email, string code)
        {
            var otp = await _context.EmailOtps
                .Where(o => o.Email == email && o.Code == code && !o.IsUsed && o.ExpiredAt >= DateTime.UtcNow)
                .OrderByDescending(o => o.CreatedAt)
                .FirstOrDefaultAsync();

            if (otp == null)
                return false;

            otp.IsUsed = true;
            otp.VerifiedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        public Task<bool> IsVerifiedAsync(string email)
        {
            var now = DateTime.UtcNow;
            return _context.EmailOtps.AnyAsync(o => o.Email == email && o.IsUsed && o.ExpiredAt >= now);
        }

        private string GenerateCode()
        {
            var random = new Random();
            return random.Next(0, 999999).ToString("D" + OtpLength);
        }
    }
}
