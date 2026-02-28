namespace ClinicManagement.Api.Dtos.User
{
    public class CreateAccountRequest
    {
        public string Username { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string Role { get; set; } = null!;
    }
}
