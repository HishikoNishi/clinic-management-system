using System;
using ClinicManagement.Api.Data;
using ClinicManagement.Api.Dtos.ClinicalTests;
using ClinicManagement.Api.DTOs;
using ClinicManagement.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace ClinicManagement.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClinicalTestsController : ControllerBase
    {
        private readonly ClinicDbContext _context;

        public ClinicalTestsController(ClinicDbContext context)
        {
            _context = context;
        }

        // Doctor t?o yêu c?u xét nghi?m
        [HttpPost]
        public async Task<IActionResult> CreateTest(CreateClinicalTestDto dto)
        {
            var test = new ClinicalTest
            {
                MedicalRecordId = dto.MedicalRecordId,
                TestName = dto.TestName
            };

            _context.ClinicalTests.Add(test);
            await _context.SaveChangesAsync();

            return Ok(test);
        }

        // Technician nh?p k?t qu?
        [HttpPatch("{id}/result")]
        public async Task<IActionResult> UpdateResult(int id, UpdateClinicalTestResultDto dto)
        {
            var test = await _context.ClinicalTests.FindAsync(id);

            if (test == null)
                return NotFound();

            test.Result = dto.Result;
            test.TechnicianName = dto.TechnicianName;

            await _context.SaveChangesAsync();

            return Ok(test);
        }

      
        [HttpGet("medical-record/{medicalRecordId}")]
        public async Task<IActionResult> GetByMedicalRecord(Guid medicalRecordId)
        {
            var tests = await _context.ClinicalTests
                .Where(t => t.MedicalRecordId == medicalRecordId)
                .ToListAsync();

            return Ok(tests);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var tests = await _context.ClinicalTests.ToListAsync();
            return Ok(tests);
        }
    }
}
