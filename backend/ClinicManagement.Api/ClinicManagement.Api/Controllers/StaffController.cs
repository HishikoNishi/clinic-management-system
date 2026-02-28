using ClinicManagement.Api.Data;
using ClinicManagement.Api.Dtos.Staff;
using ClinicManagement.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class StaffsController : ControllerBase
    {
        private readonly ClinicDbContext _context;

        public StaffsController(ClinicDbContext context)
        {
            _context = context;
        }

        /* ================= GET ALL ================= */
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var staffs = await _context.Staffs
                .Include(s => s.User)
                .Select(s => new StaffDto
                {
                    Id = s.Id,
                    Code = s.Code,
                    FullName = s.FullName,
                    Position = s.Position,
                    Username = s.User.Username,
                    CreatedAt = s.CreatedAt
                })
                .ToListAsync();

            return Ok(staffs);
        }

        /* ================= GET BY ID ================= */
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> Get(Guid id)
        {
            var staff = await _context.Staffs
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (staff == null)
                return NotFound();

            return Ok(new StaffDto
            {
                Id = staff.Id,
                Code = staff.Code,
                FullName = staff.FullName,
                Position = staff.Position,
                Username = staff.User.Username,
                CreatedAt = staff.CreatedAt
            });
        }

        /* ================= CREATE ================= */
        [HttpPost]
        public async Task<IActionResult> Create(CreateStaffDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Kiểm tra User có tồn tại không
            var user = await _context.Users.FindAsync(dto.UserId);
            if (user == null)
                return BadRequest(new { message = "User not found." });

            // Kiểm tra User đã có Staff profile chưa
            if (await _context.Staffs.AnyAsync(s => s.UserId == dto.UserId))
                return BadRequest(new { message = "This user already has a staff profile." });

            // Kiểm tra Code có trùng không
            if (await _context.Staffs.AnyAsync(s => s.Code == dto.Code))
                return BadRequest(new { message = "Staff code already exists." });

            var staff = new Staff
            {
                Id = Guid.NewGuid(),
                Code = dto.Code,
                FullName = dto.FullName,
                Position = dto.Position ?? string.Empty,
                UserId = dto.UserId,
                CreatedAt = DateTime.UtcNow
            };

            await _context.Staffs.AddAsync(staff);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(Get), new { id = staff.Id }, new
            {
                message = "Staff profile created successfully",
                staffId = staff.Id
            });
        }

        /* ================= UPDATE ================= */
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, UpdateStaffDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var staff = await _context.Staffs.FindAsync(id);
            if (staff == null)
                return NotFound();

            if (staff.Code != dto.Code &&
                await _context.Staffs.AnyAsync(s => s.Code == dto.Code))
                return BadRequest(new { message = "Staff code already exists." });

            staff.Code = dto.Code;
            staff.FullName = dto.FullName;
            staff.Position = dto.Position ?? string.Empty;
            staff.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Staff profile updated successfully." });
        }

        /* ================= DELETE ================= */
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var staff = await _context.Staffs.FindAsync(id);
            if (staff == null)
                return NotFound();

            _context.Staffs.Remove(staff);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Staff profile deleted successfully." });
        }
    }
}