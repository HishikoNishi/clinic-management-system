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

        [HttpGet("Departments")]
        public IActionResult GetDepartments()
        {
            var values = Enum.GetValues(typeof(DepartmentEnum))
                  .Cast<DepartmentEnum>()
                  .Select(e => new {
                      id = (int)e,
                      name = e.GetType()
                              .GetMember(e.ToString())
                              .First()
                              .GetCustomAttribute<DisplayAttribute>()?.Name ?? e.ToString()
                  });

            return Ok(values); 
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] CreateDepartmentDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var department = new Department
            {
                Id = Guid.NewGuid(),
                Name = dto.Name,
                Description = dto.Description,
                CreatedAt = DateTime.Now
            };

            _context.Departments.Add(department);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetDepartments), new { id = department.Id }, department);
        }
    }
}
