using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClinicManagement.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SecureController : ControllerBase
    {
        [HttpGet("public")]
        public IActionResult Public() => Ok(new { message = "Public endpoint" });

        [Authorize]
        [HttpGet("authenticated")]
        public IActionResult Authenticated() => Ok(new { message = "Authenticated endpoint", user = User.Identity?.Name });

        [Authorize(Roles = "Admin")]
        [HttpGet("admin")]
        public IActionResult AdminOnly() => Ok(new { message = "Admin-only endpoint" });
    }
}
