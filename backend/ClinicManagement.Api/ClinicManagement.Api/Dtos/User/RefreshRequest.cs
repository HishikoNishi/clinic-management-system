using System.ComponentModel.DataAnnotations;

namespace ClinicManagement.Api.Dtos.User
{
    public class RefreshRequest
    {
        [Required]
        public string RefreshToken { get; set; } = string.Empty;
    }
}
