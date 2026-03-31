using ClinicManagement.Api.Data;
using ClinicManagement.Api.Dtos.Specialty;
using ClinicManagement.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class SpecialtiesController : ControllerBase
    {
        private readonly ClinicDbContext _context;

        public SpecialtiesController(ClinicDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] Guid? departmentId)
        {
            var query = _context.Specialties.AsQueryable();
            if (departmentId.HasValue && departmentId.Value != Guid.Empty)
            {
                query = query.Where(s => s.DepartmentId == departmentId.Value);
            }

            var items = await query
                .Select(s => new
                {
                    s.Id,
                    s.Name,
                    s.DepartmentId,
                    DepartmentName = s.Department != null ? s.Department.Name : null
                })
                .ToListAsync();

            return Ok(items);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var s = await _context.Specialties
                .Include(x => x.Department)
                .FirstOrDefaultAsync(x => x.Id == id);
            if (s == null) return NotFound(new { message = "Specialty not found." });
            return Ok(new
            {
                s.Id,
                s.Name,
                s.DepartmentId,
                DepartmentName = s.Department?.Name
            });
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateSpecialtyDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var department = await _context.Departments.FindAsync(dto.DepartmentId);
            if (department == null) return BadRequest(new { message = "Department not found." });

            var exists = await _context.Specialties.AnyAsync(s =>
                s.DepartmentId == dto.DepartmentId && s.Name == dto.Name);
            if (exists) return BadRequest(new { message = "Specialty name already exists in this department." });

            var s = new Specialty
            {
                Id = Guid.NewGuid(),
                Name = dto.Name,
                DepartmentId = dto.DepartmentId
            };
            _context.Specialties.Add(s);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = s.Id }, s);
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateSpecialtyDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var specialty = await _context.Specialties.FindAsync(id);
            if (specialty == null) return NotFound(new { message = "Specialty not found." });

            var department = await _context.Departments.FindAsync(dto.DepartmentId);
            if (department == null) return BadRequest(new { message = "Department not found." });

            var exists = await _context.Specialties.AnyAsync(s =>
                s.DepartmentId == dto.DepartmentId && s.Name == dto.Name && s.Id != id);
            if (exists) return BadRequest(new { message = "Specialty name already exists in this department." });

            specialty.Name = dto.Name;
            specialty.DepartmentId = dto.DepartmentId;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Specialty updated successfully." });
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var specialty = await _context.Specialties
                .Include(s => s.Department)
                .FirstOrDefaultAsync(s => s.Id == id);
            if (specialty == null) return NotFound(new { message = "Specialty not found." });

            var hasDoctors = await _context.Doctors.AnyAsync(d => d.SpecialtyId == id);
            if (hasDoctors)
                return BadRequest(new { message = "Cannot delete: there are doctors assigned to this specialty." });

            _context.Specialties.Remove(specialty);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Specialty deleted successfully." });
        }
    }
}
