using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ClinicManagement.Api.Data;
using ClinicManagement.Api.Models;
using ClinicManagement.Api.DTOs;

namespace ClinicManagement.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PrescriptionController : ControllerBase
    {
        private readonly ClinicDbContext _context;

        public PrescriptionController(ClinicDbContext context)
        {
            _context = context;
        }

        // CREATE
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreatePrescriptionDto dto)
        {
            var prescription = new Prescription
            {
                Id = Guid.NewGuid(),
                MedicalRecordId = dto.MedicalRecordId,
                CreatedAt = DateTime.UtcNow,
                Note = dto.Note,
                PrescriptionDetails = dto.Details.Select(d => new PrescriptionDetail
                {
                    Id = Guid.NewGuid(),
                    MedicineName = d.MedicineName,
                    Dosage = d.Dosage,
                    Frequency = d.Frequency,
                    Duration = d.Quantity > 0 ? d.Quantity : d.Duration
                }).ToList()
            };

            _context.Prescriptions.Add(prescription);

            await _context.SaveChangesAsync();

            return Ok(prescription);
        }

        // GET ALL
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var prescriptions = await _context.Prescriptions
                .Include(p => p.PrescriptionDetails)
                .ToListAsync();

            return Ok(prescriptions);
        }

        // GET BY ID
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var prescription = await _context.Prescriptions
                .Include(p => p.PrescriptionDetails)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (prescription == null)
                return NotFound();

            return Ok(prescription);
        }

        // UPDATE
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] CreatePrescriptionDto dto)
        {
            var prescription = await _context.Prescriptions
                .Include(p => p.PrescriptionDetails)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (prescription == null)
                return NotFound();

            // update note
            prescription.Note = dto.Note;

            // delete old details directly from DB
            var oldDetails = await _context.PrescriptionDetails
                .Where(d => d.PrescriptionId == id)
                .ToListAsync();

            _context.PrescriptionDetails.RemoveRange(oldDetails);

            // add new details
            foreach (var d in dto.Details)
            {
                _context.PrescriptionDetails.Add(new PrescriptionDetail
                {
                    Id = Guid.NewGuid(),
                    PrescriptionId = id,
                    MedicineName = d.MedicineName,
                    Dosage = d.Dosage,
                    Frequency = d.Frequency,
                    Duration = d.Quantity > 0 ? d.Quantity : d.Duration
                });
            }

            await _context.SaveChangesAsync();

            return Ok("Updated successfully");
        }

        // DELETE
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var prescription = await _context.Prescriptions
                .Include(p => p.PrescriptionDetails)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (prescription == null)
                return NotFound();

            _context.PrescriptionDetails.RemoveRange(prescription.PrescriptionDetails);

            _context.Prescriptions.Remove(prescription);

            await _context.SaveChangesAsync();

            return Ok("Deleted successfully");
        }
    }
}
