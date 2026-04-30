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
    [Route("api/staffs")]
    public class StaffMediaController : ControllerBase
    {
        private readonly ClinicDbContext _context;
        private readonly IWebHostEnvironment _env;
        private const long MaxFileSize = 2 * 1024 * 1024; // 2MB

        public StaffMediaController(ClinicDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        [HttpPost("{id}/avatar")]
        [Authorize(Roles = "Admin,Staff")]
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

                var staff = await _context.Staffs.FirstOrDefaultAsync(s => s.Id == id);
                if (staff == null)
                    return NotFound(new { message = "Khong tim thay nhan vien" });

                if (User.IsInRole("Staff"))
                {
                    var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                    if (string.IsNullOrWhiteSpace(userIdClaim) ||
                        !Guid.TryParse(userIdClaim, out var claimUserId) ||
                        staff.UserId != claimUserId)
                    {
                        return Forbid();
                    }
                }

                var webRoot = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                var folder = Path.Combine(webRoot, "uploads", "staffs");
                Directory.CreateDirectory(folder);

                var code = string.IsNullOrWhiteSpace(staff.Code) ? $"staff-{staff.Id:N}" : staff.Code;
                var safeCode = string.Concat(code.Where(ch => !Path.GetInvalidFileNameChars().Contains(ch)));
                if (string.IsNullOrWhiteSpace(safeCode))
                {
                    safeCode = $"staff-{staff.Id:N}";
                }

                var fileName = $"{safeCode}-{Guid.NewGuid():N}{ext}";
                var savePath = Path.Combine(folder, fileName);

                await using (var stream = System.IO.File.Create(savePath))
                {
                    await file.CopyToAsync(stream);
                }

                var publicUrl = $"{Request.Scheme}://{Request.Host}/uploads/staffs/{fileName}";
                staff.AvatarUrl = publicUrl;
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
