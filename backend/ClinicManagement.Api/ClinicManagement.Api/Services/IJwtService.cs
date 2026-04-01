using ClinicManagement.Api.Models;

namespace ClinicManagement.Api.Services
{
    public interface IJwtService
    {
        string GenerateToken(User user);
    }
}
