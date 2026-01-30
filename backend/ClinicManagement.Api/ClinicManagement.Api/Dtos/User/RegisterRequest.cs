namespace ClinicManagement.Api.Dtos.User
{
    public class RegisterRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        // Optional: allow specifying role at registration (be careful in production)
        public string Role { get; set; } = "User";
    }
}
