using ClinicManagement.Api.Data;
using ClinicManagement.Api.Dtos.Patient;
using ClinicManagement.Api.Dtos.Patients;
using ClinicManagement.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PatientController : ControllerBase
    {
        private readonly ClinicDbContext _context;

        public PatientController(ClinicDbContext context)
        {
            _context = context;
        }

        // =============================
        // GET ALL
        // =============================
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PatientDto>>> GetAll()
        {
            var patients = await _context.Patients
                .Where(p => !p.IsDeleted)
                .OrderByDescending(p => p.CreatedAt)
                .Select(p => new PatientDto
                {
                    Id = p.Id,
                    FullName = p.FullName,
                    DateOfBirth = p.DateOfBirth,
                    Gender = p.Gender,
                    Phone = p.Phone,
                    Email = p.Email,
                    Address = p.Address,
                    Note = p.Note
                })
                .ToListAsync();

            return Ok(patients);
        }

        // =============================
        // GET BY ID
        // =============================
        [HttpGet("{id}")]
        public async Task<ActionResult<PatientDto>> GetById(Guid id)
        {
            var patient = await _context.Patients
                .Where(p => !p.IsDeleted && p.Id == id)
                .Select(p => new PatientDto
                {
                    Id = p.Id,
                    FullName = p.FullName,
                    DateOfBirth = p.DateOfBirth,
                    Gender = p.Gender,
                    Phone = p.Phone,
                    Email = p.Email,
                    Address = p.Address,
                    Note = p.Note
                })
                .FirstOrDefaultAsync();

            if (patient == null)
                return NotFound("Patient not found");

            return Ok(patient);
        }

        // =============================
        // CREATE
        // =============================
        [HttpPost]
        public async Task<ActionResult> Create(CreatePatientDto dto)
        {
            var patient = new Patient
            {
                Id = Guid.NewGuid(),
                FullName = dto.FullName,
                DateOfBirth = dto.DateOfBirth,
                Gender = dto.Gender,
                Phone = dto.Phone,
                Email = dto.Email,
                Address = dto.Address,
                Note = dto.Note,
                CreatedAt = DateTime.UtcNow
            };

            _context.Patients.Add(patient);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = patient.Id }, null);
        }

        // =============================
        // UPDATE
        // =============================
        [HttpPut("{id}")]
        public async Task<ActionResult> Update(Guid id, UpdatePatientDto dto)
        {
            var patient = await _context.Patients
                .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);

            if (patient == null)
                return NotFound("Patient not found");

            patient.FullName = dto.FullName;
            patient.DateOfBirth = dto.DateOfBirth;
            patient.Gender = dto.Gender;
            patient.Phone = dto.Phone;
            patient.Email = dto.Email;
            patient.Address = dto.Address;
            patient.Note = dto.Note;
            patient.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // =============================
        // DELETE (Soft Delete)
        // =============================
        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(Guid id)
        {
            var patient = await _context.Patients
                .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);

            if (patient == null)
                return NotFound("Patient not found");

            patient.IsDeleted = true;
            patient.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // =============================
        // SEARCH
        // =============================
        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<PatientDto>>> Search(string? keyword)
        {
            var query = _context.Patients
                .Where(p => !p.IsDeleted);

            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(p =>
                    p.FullName.Contains(keyword) ||
                    (p.Phone != null && p.Phone.Contains(keyword)) ||
                    (p.Email != null && p.Email.Contains(keyword))
                );
            }

            var result = await query
                .Select(p => new PatientDto
                {
                    Id = p.Id,
                    FullName = p.FullName,
                    DateOfBirth = p.DateOfBirth,
                    Gender = p.Gender,
                    Phone = p.Phone,
                    Email = p.Email,
                    Address = p.Address,
                    Note = p.Note
                })
                .ToListAsync();

            return Ok(result);
        }
    }
}