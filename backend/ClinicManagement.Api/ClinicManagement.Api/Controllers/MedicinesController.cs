using System;
using ClinicManagement.Api.Data;
using ClinicManagement.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MedicinesController : ControllerBase
    {
        private readonly ClinicDbContext _context;

        public MedicinesController(ClinicDbContext context)
        {
            _context = context;
        }

        // Lấy toàn bộ danh mục thuốc (Cho màn hình quản lý)
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await _context.Medicines.ToListAsync());
        }

        // THÊM THUỐC MỚI
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Medicine medicine)
        {
            medicine.Id = Guid.NewGuid();
            _context.Medicines.Add(medicine);
            await _context.SaveChangesAsync();
            return Ok(medicine);
        }

        // CẬP NHẬT THUỐC
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] Medicine updatedMedicine)
        {
            var medicine = await _context.Medicines.FindAsync(id);
            if (medicine == null) return NotFound();

            medicine.Name = updatedMedicine.Name;
            medicine.DefaultDosage = updatedMedicine.DefaultDosage;
            medicine.Unit = updatedMedicine.Unit;
            medicine.Price = updatedMedicine.Price;
            medicine.IsActive = updatedMedicine.IsActive;

            await _context.SaveChangesAsync();
            return Ok("Cập nhật thành công");
        }
        [HttpGet("suggest")]
        public async Task<IActionResult> Suggest([FromQuery] string q)
        {
            if (string.IsNullOrWhiteSpace(q)) return Ok(new List<object>());
            var query = q.Trim().ToLower();

            var suggestions = await _context.Medicines
                .Where(m => m.IsActive && m.Name.ToLower().Contains(query))
                .OrderBy(m => m.Name)
                .Take(15)
                .Select(m => new {
                    m.Id,
                    m.Name,
                    m.DefaultDosage,
                    m.Unit,
                    m.Price,
                    DisplayText = $"{m.Name} {m.DefaultDosage} ({m.Unit})"
                })
                .ToListAsync();

            return Ok(suggestions);
        }
        // XÓA THUỐC (Hoặc ẩn đi bằng IsActive)
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var medicine = await _context.Medicines.FindAsync(id);
            if (medicine == null) return NotFound();

            _context.Medicines.Remove(medicine);
            await _context.SaveChangesAsync();
            return Ok("Đã xóa thuốc khỏi danh mục");
        }
    }
}

