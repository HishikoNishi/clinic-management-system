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
    public class StaffsController : ControllerBase
    {
        private readonly ClinicDbContext _context;

        public StaffsController(ClinicDbContext context)
        {
            _context = context;
        }

        /* ================= GET ALL ================= */
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var staffs = await _context.Staffs
                .Include(s => s.User)
                .Include(s => s.Department)
                .Select(s => new StaffDto
                {
                    Id = s.Id,
                    Code = s.Code,
                    FullName = s.FullName,
                    Role = s.Role,
                    IsActive = s.IsActive,
                    Username = s.User.Username,
                    UserId = s.UserId,
                    CreatedAt = s.CreatedAt,
                    AvatarUrl = s.AvatarUrl,
                    DepartmentId = s.DepartmentId,
                    DepartmentName = s.Department != null ? s.Department.Name : null
                })
                .ToListAsync();

            return Ok(staffs);
        }

        /* ================= GET BY ID ================= */
        [Authorize(Roles = "Admin")]
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> Get(Guid id)
        {
            var staff = await _context.Staffs
                .Include(s => s.User)
                .Include(s => s.Department)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (staff == null)
                return NotFound();

            return Ok(new StaffDto
            {
                Id = staff.Id,
                Code = staff.Code,
                FullName = staff.FullName,
                Role = staff.Role,
                IsActive = staff.IsActive,
                Username = staff.User.Username,
                UserId = staff.UserId,
                CreatedAt = staff.CreatedAt,
                DepartmentId = staff.DepartmentId,
                DepartmentName = staff.Department != null ? staff.Department.Name : null
            });
        }

        /* ================= CREATE ================= */
        [Authorize(Roles = "Admin")]
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

            if (dto.Role == "Technician" && dto.DepartmentId == null)
                return BadRequest(new { message = "Technician requires a department." });
            if (dto.DepartmentId.HasValue && !await _context.Departments.AnyAsync(d => d.Id == dto.DepartmentId.Value))
                return BadRequest(new { message = "Department not found." });

            var staff = new Staff
            {
                Id = Guid.NewGuid(),
                Code = dto.Code,
                FullName = dto.FullName,
                Role = dto.Role,
                UserId = dto.UserId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                DepartmentId = dto.DepartmentId
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
        [Authorize(Roles = "Admin")]
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

            if (dto.Role == "Technician" && dto.DepartmentId == null)
                return BadRequest(new { message = "Technician requires a department." });
            if (dto.DepartmentId.HasValue && !await _context.Departments.AnyAsync(d => d.Id == dto.DepartmentId.Value))
                return BadRequest(new { message = "Department not found." });

            staff.Code = dto.Code;
            staff.FullName = dto.FullName;
            staff.Role = dto.Role;
            staff.UserId = dto.UserId;
            staff.IsActive = dto.IsActive;
            staff.DepartmentId = dto.DepartmentId;
            staff.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Staff profile updated successfully." });
        }

        /* ================= DELETE ================= */
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var staff = await _context.Staffs.FindAsync(id);
            if (staff == null)
                return NotFound();

            staff.IsDeleted = true;
            staff.IsActive = false;
            staff.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Staff profile deleted successfully (soft delete)." });
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("{id:guid}/restore")]
        public async Task<IActionResult> Restore(Guid id)
        {
            var staff = await _context.Staffs
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(s => s.Id == id);

            if (staff == null)
                return NotFound();

            if (!staff.IsDeleted)
                return BadRequest(new { message = "Staff is not deleted." });

            staff.IsDeleted = false;
            staff.IsActive = true;
            staff.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Staff restored successfully." });
        }
        [HttpGet("profile")]
        [Authorize(Roles = "Staff")]
        public async Task<IActionResult> GetMyProfile()
        {
            // Lấy userId từ token (claim NameIdentifier)
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim)) return Unauthorized();

            var userId = Guid.Parse(userIdClaim);

            var staff = await _context.Staffs
                .Include(s => s.User)
                .Include(s => s.Department)
                .FirstOrDefaultAsync(s => s.UserId == userId);

            if (staff == null) return NotFound(new { message = "Staff profile not found" });

            return Ok(new StaffDto
            {
                Id = staff.Id,
                Code = staff.Code,
                FullName = staff.FullName,
                Role = staff.Role,
                IsActive = staff.IsActive,
                Username = staff.User.Username,
                UserId = staff.UserId,
                CreatedAt = staff.CreatedAt,
                AvatarUrl = staff.AvatarUrl,
                DepartmentId = staff.DepartmentId,
                DepartmentName = staff.Department != null ? staff.Department.Name : null
            });
        }
        // PUT api/Staffs/profile
        [HttpPut("profile")]
        [Authorize(Roles = "Staff")]
        public async Task<IActionResult> UpdateMyProfile(UpdateMyStaffDto dto)
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim)) return Unauthorized();

            var userId = Guid.Parse(userIdClaim);

            var staff = await _context.Staffs.FirstOrDefaultAsync(s => s.UserId == userId);
            if (staff == null) return NotFound();

            staff.Code = dto.Code;
            staff.FullName = dto.FullName;
            staff.IsActive = dto.IsActive;
            staff.AvatarUrl = dto.AvatarUrl;
            staff.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Profile updated successfully" });
        }

    }
}
