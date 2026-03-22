using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ClinicManagement.Api.Data;
using ClinicManagement.Api.Models;
using Microsoft.AspNetCore.Authorization;
using ClinicManagement.Api.Dtos.Department;
using ClinicManagement.Api.Dtos.Doctor;

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
            var deps = await _context.Departments
                .Include(d => d.Doctors)
                    .ThenInclude(doc => doc.User) 
                .Select(d => new DepartmentDto
                {
                    Id = d.Id,
                    Name = d.Name,
                    Description = d.Description,
                    CreatedAt = d.CreatedAt,
                    Doctors = d.Doctors.Select(doc => new DoctorDto
                    {
                        Id = doc.Id,
                        Code = doc.Code,
                        FullName = doc.FullName,
                        Specialty = doc.Specialty,
                        LicenseNumber = doc.LicenseNumber,
                        Username = doc.User != null ? doc.User.Username : null, 
                        Status = doc.Status.ToString() 
                    }).ToList()

                })
                .ToListAsync();

            return Ok(deps);
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