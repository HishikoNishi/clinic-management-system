using ClinicManagement.Api.Data;
using ClinicManagement.Api.Dtos.Medicines;
using ClinicManagement.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    // Master data for medicine catalog used by doctor suggestion and prescription pricing.
    public class MedicinesController : ControllerBase
    {
        private readonly ClinicDbContext _context;

        public MedicinesController(ClinicDbContext context)
        {
            _context = context;
        }

        // Admin xem full danh mục; role khác chỉ xem active.
        [HttpGet]
        [Authorize(Roles = "Admin,Doctor,Staff")]
        public async Task<IActionResult> GetAll([FromQuery] bool includeInactive = false)
        {
            var isAdmin = User.IsInRole("Admin");

            IQueryable<Medicine> query = _context.Medicines.AsNoTracking();
            if (!isAdmin || !includeInactive)
            {
                query = query.Where(m => m.IsActive);
            }

            var data = await query
                .OrderBy(m => m.Name)
                .Select(m => new
                {
                    m.Id,
                    m.Name,
                    m.DefaultDosage,
                    m.Unit,
                    m.Price,
                    m.IsActive
                })
                .ToListAsync();

            return Ok(data);
        }

        [HttpGet("suggest")]
        [Authorize(Roles = "Admin,Doctor,Staff")]
        public async Task<IActionResult> Suggest([FromQuery] string q)
        {
            if (string.IsNullOrWhiteSpace(q)) return Ok(new List<object>());

            var keyword = q.Trim().ToLower();
            var suggestions = await _context.Medicines
                .AsNoTracking()
                .Where(m => m.IsActive && m.Name.ToLower().Contains(keyword))
                .OrderBy(m => m.Name)
                .Take(15)
                .Select(m => new
                {
                    m.Id,
                    m.Name,
                    m.DefaultDosage,
                    m.Unit,
                    m.Price,
                    DisplayText = $"{m.Name} {(string.IsNullOrWhiteSpace(m.DefaultDosage) ? "" : m.DefaultDosage)} ({m.Unit})".Trim()
                })
                .ToListAsync();

            return Ok(suggestions);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] UpsertMedicineDto dto)
        {
            var normalizedName = (dto.Name ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(normalizedName))
                return BadRequest(new { message = "Tên thuốc là bắt buộc" });

            var exists = await _context.Medicines.AnyAsync(m => m.Name.ToLower() == normalizedName.ToLower());
            if (exists)
                return BadRequest(new { message = "Thuốc đã tồn tại trong danh mục" });

            var medicine = new Medicine
            {
                Id = Guid.NewGuid(),
                Name = normalizedName,
                DefaultDosage = string.IsNullOrWhiteSpace(dto.DefaultDosage) ? null : dto.DefaultDosage.Trim(),
                Unit = string.IsNullOrWhiteSpace(dto.Unit) ? "Vien" : dto.Unit.Trim(),
                Price = dto.Price,
                IsActive = dto.IsActive
            };

            _context.Medicines.Add(medicine);
            await _context.SaveChangesAsync();

            return Ok(medicine);
        }

        [HttpPut("{id:guid}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpsertMedicineDto dto)
        {
            var medicine = await _context.Medicines.FindAsync(id);
            if (medicine == null) return NotFound(new { message = "Không tìm thấy thuốc" });

            var normalizedName = (dto.Name ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(normalizedName))
                return BadRequest(new { message = "Tên thuốc là bắt buộc" });
            var exists = await _context.Medicines.AnyAsync(m =>
                m.Id != id && m.Name.ToLower() == normalizedName.ToLower());

            if (exists)
                return BadRequest(new { message = "Tên thuốc đã tồn tại" });

            medicine.Name = normalizedName;
            medicine.DefaultDosage = string.IsNullOrWhiteSpace(dto.DefaultDosage) ? null : dto.DefaultDosage.Trim();
            medicine.Unit = string.IsNullOrWhiteSpace(dto.Unit) ? "Viên" : dto.Unit.Trim();
            medicine.Price = dto.Price;
            medicine.IsActive = dto.IsActive;

            await _context.SaveChangesAsync();
            return Ok(new { message = "Cập nhật thuốc thành công" });
        }

        [HttpPatch("{id:guid}/toggle")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ToggleStatus(Guid id)
        {
            var medicine = await _context.Medicines.FindAsync(id);
            if (medicine == null) return NotFound(new { message = "Không tìm thấy thuốc" });

            medicine.IsActive = !medicine.IsActive;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Cập nhật trạng thái thành công", medicine.IsActive });
        }

        // Soft delete: deactivate thay vi xoa vat ly de tranh mat lich su don thuoc.
        [HttpDelete("{id:guid}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var medicine = await _context.Medicines
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(m => m.Id == id);
            if (medicine == null) return NotFound(new { message = "Không tìm thấy thuốc" });

            medicine.IsDeleted = true;
            medicine.IsActive = false;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Đã xóa thuốc (soft delete)" });
        }

        [HttpPost("{id:guid}/restore")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Restore(Guid id)
        {
            var medicine = await _context.Medicines
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(m => m.Id == id);
            if (medicine == null) return NotFound(new { message = "Không tìm thấy thuốc" });

            if (!medicine.IsDeleted)
                return BadRequest(new { message = "Thuốc chưa bị xóa" });

            medicine.IsDeleted = false;
            medicine.IsActive = true;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Đã khôi phục thuốc" });
        }
    }
}
