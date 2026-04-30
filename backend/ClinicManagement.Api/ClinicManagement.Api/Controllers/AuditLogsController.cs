using ClinicManagement.Api.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class AuditLogsController : ControllerBase
    {
        private readonly ClinicDbContext _context;

        public AuditLogsController(ClinicDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        // Admin review endpoint: filter by entity/action/date and read newest changes first.
        public async Task<IActionResult> Get(
            [FromQuery] string? entityName,
            [FromQuery] string? action,
            [FromQuery] DateTime? from,
            [FromQuery] DateTime? to,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            page = page < 1 ? 1 : page;
            pageSize = pageSize < 1 ? 20 : Math.Min(pageSize, 100);

            var query = _context.AuditLogs.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(entityName))
                query = query.Where(x => x.EntityName == entityName.Trim());
            if (!string.IsNullOrWhiteSpace(action))
                query = query.Where(x => x.Action == action.Trim());
            if (from.HasValue)
                query = query.Where(x => x.ChangedAt >= from.Value);
            if (to.HasValue)
            {
                var toInclusive = to.Value.Date.AddDays(1);
                query = query.Where(x => x.ChangedAt < toInclusive);
            }

            var total = await query.CountAsync();
            var items = await query
                .OrderByDescending(x => x.ChangedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new
                {
                    x.Id,
                    x.Action,
                    x.EntityName,
                    x.RecordId,
                    x.UserId,
                    x.Username,
                    x.BeforeData,
                    x.AfterData,
                    x.ChangedAt
                })
                .ToListAsync();

            return Ok(new
            {
                page,
                pageSize,
                total,
                items
            });
        }
    }
}
