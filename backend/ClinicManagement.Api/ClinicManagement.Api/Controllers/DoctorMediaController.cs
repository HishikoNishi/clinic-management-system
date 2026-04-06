using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ClinicManagement.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Api.Controllers
{
    [ApiController]
    [Route("api/Doctor")]
    public class DoctorMediaController : ControllerBase
    {
        private readonly ClinicDbContext _context;
        private readonly IWebHostEnvironment _env;
        private readonly long _maxFileSize = 2 * 1024 * 1024; // 2MB

        public DoctorMediaController(ClinicDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        [HttpPost("{id}/avatar")]
        [Authorize(Roles = "Admin")]
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

            var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.Id == id);
            if (doctor == null) return NotFound(new { message = "Không tìm thấy bác sĩ" });

            var webRoot = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            var folder = Path.Combine(webRoot, "uploads", "doctors");
            Directory.CreateDirectory(folder);

            var fileName = $"{doctor.Code}-{Guid.NewGuid():N}{ext}";
            var savePath = Path.Combine(folder, fileName);

            using (var stream = System.IO.File.Create(savePath))
            {
                await file.CopyToAsync(stream);
            }

            var publicUrl = $"{Request.Scheme}://{Request.Host}/uploads/doctors/{fileName}";
            doctor.AvatarUrl = publicUrl;
            await _context.SaveChangesAsync();

            return Ok(new { url = publicUrl });
        }
    }
}
