namespace ClinicManagement.Api.Dtos.User
{
    public class AuthResponse
    {
        public string Token { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
        public string RefreshToken { get; set; } = string.Empty;
        public DateTime RefreshExpiresAt { get; set; }
        public Guid? DoctorId { get; set; }
    }
}
