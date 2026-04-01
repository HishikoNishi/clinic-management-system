using System.Text.Json.Serialization;

namespace ClinicManagement.Api.Models
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum DoctorStatus
    {
        Active = 1,    
        Busy = 2,     
        Inactive = 3   
    }
}
