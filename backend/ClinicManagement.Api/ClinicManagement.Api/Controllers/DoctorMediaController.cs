using ClinicManagement.Api.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Linq;

namespace ClinicManagement.Api.Controllers
{
    [ApiController]
    [Route("api/doctor")]
    public class DoctorMediaController : ControllerBase
    {
        private readonly ClinicDbContext _context;
        private readonly IWebHostEnvironment _env;
        private const long MaxFileSize = 2 * 1024 * 1024; // 2MB

        public DoctorMediaController(ClinicDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        [HttpPost("{id}/avatar")]
        [Authorize(Roles = "Admin,Doctor")]
        public async Task<IActionResult> UploadAvatar(Guid id, [FromForm] IFormFile? file)
        {
            try
            {
                if (file == null || file.Length == 0)
                    return BadRequest(new { message = "File trong" });

                if (file.Length > MaxFileSize)
                    return BadRequest(new { message = "Kich thuoc anh toi da 2MB" });

                var allowed = new[] { ".jpg", ".jpeg", ".png", ".webp" };
                var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
                if (!allowed.Contains(ext))
                    return BadRequest(new { message = "Dinh dang anh khong hop le (jpg, png, webp)" });

                var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.Id == id);
                if (doctor == null)
                    return NotFound(new { message = "Khong tim thay bac si" });

                if (User.IsInRole("Doctor"))
                {
                    var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                    if (string.IsNullOrWhiteSpace(userIdClaim) ||
                        !Guid.TryParse(userIdClaim, out var claimUserId) ||
                        doctor.UserId != claimUserId)
                    {
                        return Forbid();
                    }
                }

                var webRoot = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                var folder = Path.Combine(webRoot, "uploads", "doctors");
                Directory.CreateDirectory(folder);

                var code = string.IsNullOrWhiteSpace(doctor.Code) ? $"doctor-{doctor.Id:N}" : doctor.Code;
                var safeCode = string.Concat(code.Where(ch => !Path.GetInvalidFileNameChars().Contains(ch)));
                if (string.IsNullOrWhiteSpace(safeCode))
                {
                    safeCode = $"doctor-{doctor.Id:N}";
                }

                var fileName = $"{safeCode}-{Guid.NewGuid():N}{ext}";
                var savePath = Path.Combine(folder, fileName);

                await using (var stream = System.IO.File.Create(savePath))
                {
                    await file.CopyToAsync(stream);
                }

                var publicUrl = $"{Request.Scheme}://{Request.Host}/uploads/doctors/{fileName}";
                doctor.AvatarUrl = publicUrl;
                await _context.SaveChangesAsync();

                return Ok(new { url = publicUrl });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Upload avatar that bai", detail = ex.Message });
            }
        }
    }
}
