using System.ComponentModel.DataAnnotations;

namespace ClinicManagement.Api.Dtos.Otp
{
    public class SendOtpRequestDto
    {
        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;
    }
}
