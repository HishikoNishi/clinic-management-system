using ClinicManagement.Api.Data;
using ClinicManagement.Api.Models;
using ClinicManagement.Api.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ClinicManagement.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MedicalRecordsController : ControllerBase
    {
        private readonly ClinicDbContext _context;

        public MedicalRecordsController(ClinicDbContext context)
        {
            _context = context;
        }

        // GET ALL
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var records = await _context.MedicalRecords
                .Select(r => new MedicalRecordResponseDto
                {
                    Id = r.Id,
                    PatientId = r.PatientId,
                    DoctorId = r.DoctorId,
                    Symptoms = r.Symptoms,
                    Diagnosis = r.Diagnosis,
                    Treatment = r.Treatment,
                    Note = r.Note,
                    CreatedAt = r.CreatedAt
                })
                .ToListAsync();

            return Ok(records);
        }

        // GET BY ID
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var record = await _context.MedicalRecords
                .Where(r => r.Id == id)
                .Select(r => new MedicalRecordResponseDto
                {
                    Id = r.Id,
                    PatientId = r.PatientId,
                    DoctorId = r.DoctorId,
                    Symptoms = r.Symptoms,
                    Diagnosis = r.Diagnosis,
                    Treatment = r.Treatment,
                    Note = r.Note,
                    CreatedAt = r.CreatedAt
                })
                .FirstOrDefaultAsync();

            if (record == null)
                return NotFound();

            return Ok(record);
        }

        // CREATE
        [HttpPost]
        public async Task<IActionResult> Create(CreateMedicalRecordDto dto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userIdClaim == null)
                return Unauthorized();

            var userId = Guid.Parse(userIdClaim);

            // tìm doctor từ user
            var doctor = await _context.Doctors
                .FirstOrDefaultAsync(d => d.UserId == userId);

            if (doctor == null)
                return BadRequest("User is not a doctor");

            // tìm appointment
            var appointment = await _context.Appointments
                .FirstOrDefaultAsync(a => a.Id == dto.AppointmentId);

            if (appointment == null)
                return BadRequest("Appointment not found");

            var record = new MedicalRecord
            {
                Id = Guid.NewGuid(),
                AppointmentId = appointment.Id,
                PatientId = appointment.PatientId,
                DoctorId = doctor.Id,
                Symptoms = dto.Symptoms,
                Diagnosis = dto.Diagnosis,
                Treatment = dto.Treatment,
                Note = dto.Note,
                CreatedAt = DateTime.UtcNow
            };

            _context.MedicalRecords.Add(record);
            await _context.SaveChangesAsync();

            return Ok(new MedicalRecordResponseDto
            {
                Id = record.Id,
                PatientId = record.PatientId,
                DoctorId = record.DoctorId,
                Symptoms = record.Symptoms,
                Diagnosis = record.Diagnosis,
                Treatment = record.Treatment,
                Note = record.Note,
                CreatedAt = record.CreatedAt
            });
        }

        // UPDATE
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, UpdateMedicalRecordDto dto)
        {
            var record = await _context.MedicalRecords.FindAsync(id);

            if (record == null)
                return NotFound();

            record.Symptoms = dto.Symptoms;
            record.Diagnosis = dto.Diagnosis;
            record.Treatment = dto.Treatment;
            record.Note = dto.Note;

            await _context.SaveChangesAsync();

            return Ok(new MedicalRecordResponseDto
            {
                Id = record.Id,
                PatientId = record.PatientId,
                DoctorId = record.DoctorId,
                Symptoms = record.Symptoms,
                Diagnosis = record.Diagnosis,
                Treatment = record.Treatment,
                Note = record.Note,
                CreatedAt = record.CreatedAt
            });
        }

        // DELETE
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var record = await _context.MedicalRecords.FindAsync(id);

            if (record == null)
                return NotFound();

            _context.MedicalRecords.Remove(record);
            await _context.SaveChangesAsync();

            return Ok("Deleted successfully");
        }
    }
}