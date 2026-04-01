using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ClinicManagement.Api.Data;
using ClinicManagement.Api.Models;
using Microsoft.AspNetCore.Authorization;
using ClinicManagement.Api.Dtos.Department;
using ClinicManagement.Api.Dtos.Doctor;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace ClinicManagement.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DepartmentsController : ControllerBase
    {
        private readonly ClinicDbContext _context;

        public DepartmentsController(ClinicDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetDepartments()
        {
            var departments = await _context.Departments
                .Select(d => new {
                    id = d.Id,
                    name = d.Name,
                    description = d.Description
                })
                .ToListAsync();

            return Ok(departments);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var dep = await _context.Departments.FindAsync(id);
            if (dep == null) return NotFound(new { message = "Department not found." });
            return Ok(new { id = dep.Id, dep.Name, dep.Description });
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] CreateDepartmentDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var exists = await _context.Departments.AnyAsync(d => d.Name == dto.Name);
            if (exists)
                return BadRequest(new { message = "Department name already exists." });

            var department = new Department
            {
                Id = Guid.NewGuid(),
                Name = dto.Name,
                Description = dto.Description,
                CreatedAt = DateTime.UtcNow
            };

            _context.Departments.Add(department);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetDepartments), new { id = department.Id }, department);
        }
        [HttpPut("{id:guid}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateDepartment(Guid id, [FromBody] UpdateDepartmentDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var department = await _context.Departments.FindAsync(id);
            if (department == null)
                return NotFound(new { message = "Department not found." });

            var exists = await _context.Departments.AnyAsync(d => d.Name == dto.Name && d.Id != id);
            if (exists)
                return BadRequest(new { message = "Department name already exists." });

            department.Name = dto.Name;
            department.Description = dto.Description;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Department updated successfully." });
        }

        [HttpDelete("{id:guid}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var department = await _context.Departments
                .Include(d => d.Doctors)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (department == null)
                return NotFound(new { message = "Department not found." });

            if (department.Doctors.Any())
                return BadRequest(new { message = "Cannot delete: there are doctors assigned to this department." });

            _context.Departments.Remove(department);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Department deleted successfully." });
        }
    }
}
