namespace ClinicManagement.Api.Models
{
    public class ApiErrorResponse
    {
        public string Code { get; set; } = "bad_request";
        public string Message { get; set; } = "Request failed.";
        public object? Details { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.Now;
    }
}
