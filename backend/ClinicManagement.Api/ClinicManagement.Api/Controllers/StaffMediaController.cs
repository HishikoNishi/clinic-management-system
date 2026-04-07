using ClinicManagement.Api.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;

namespace ClinicManagement.Api.Controllers
{
    [ApiController]
    [Route("api/Staffs")]
    public class StaffMediaController : ControllerBase
    {
        private readonly ClinicDbContext _context;
        private readonly IWebHostEnvironment _env;
        private readonly long _maxFileSize = 2 * 1024 * 1024; // 2MB

        public StaffMediaController(ClinicDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        [HttpPost("{id}/avatar")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> UploadAvatar(Guid id, IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { message = "File trống" });

            if (file.Length > _maxFileSize)
                return BadRequest(new { message = "Kích thước ảnh tối đa 2MB" });

            var allowed = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowed.Contains(ext))
                return BadRequest(new { message = "Định dạng ảnh không hợp lệ (jpg, png, webp)" });

            var staff = await _context.Staffs.FirstOrDefaultAsync(s => s.Id == id);
            if (staff == null) return NotFound(new { message = "Không tìm thấy nhân viên" });

            if (User.IsInRole("Staff"))
            {
                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || staff.UserId != Guid.Parse(userIdClaim))
                    return Forbid();
            }

            var webRoot = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            var folder = Path.Combine(webRoot, "uploads", "staffs");
            Directory.CreateDirectory(folder);

            var fileName = $"{staff.Code}-{Guid.NewGuid():N}{ext}";
            var savePath = Path.Combine(folder, fileName);

            using (var stream = System.IO.File.Create(savePath))
            {
                await file.CopyToAsync(stream);
            }

            var publicUrl = $"{Request.Scheme}://{Request.Host}/uploads/staffs/{fileName}";
            staff.AvatarUrl = publicUrl;
            await _context.SaveChangesAsync();

            return Ok(new { url = publicUrl });
        }
    }
}
