namespace ClinicManagement.Api.Dtos.User
{
    public class UpdateProfileRequest
    {
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
    }
}
