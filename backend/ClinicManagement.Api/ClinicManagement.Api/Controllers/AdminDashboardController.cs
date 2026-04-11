using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ClinicManagement.Api.Data;
using ClinicManagement.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Api.Controllers
{
    [ApiController]
    [Route("api/admin/[controller]")]
    [Authorize(Roles = "Admin")]
    public class AdminDashboardController : ControllerBase
    {
        private readonly ClinicDbContext _context;

        public AdminDashboardController(ClinicDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Thống kê tổng quan: số liệu thực từ DB + chuỗi ngày 14 ngày gần nhất cho biểu đồ.
        /// </summary>
        [HttpGet("stats")]
        public async Task<ActionResult<AdminDashboardStatsDto>> GetStats()
        {
            var today = DateTime.Today;
            var todayStart = today;
            var todayEnd = today.AddDays(1);

            var patientCount = await _context.Patients.AsNoTracking().CountAsync();
            var doctorActiveCount = await _context.Doctors.AsNoTracking()
                .CountAsync(d => d.Status == DoctorStatus.Active);

            var appointmentsToday = await _context.Appointments.AsNoTracking()
                .CountAsync(a => a.AppointmentDate >= todayStart && a.AppointmentDate < todayEnd);

            var statusCounts = await _context.Appointments.AsNoTracking()
                .GroupBy(a => a.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToListAsync();

            var byStatus = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            foreach (var s in statusCounts)
                byStatus[s.Status.ToString()] = s.Count;

            var monthStart = new DateTime(today.Year, today.Month, 1);
            var monthEnd = monthStart.AddMonths(1);

            var revenueThisMonth = await _context.Payments.AsNoTracking()
                .Where(p => p.PaymentDate >= monthStart && p.PaymentDate < monthEnd)
                .SumAsync(p => p.Amount);

            var seriesStart = today.AddDays(-13);
            var rangeEnd = today.AddDays(1);
            var countsByDay = await _context.Appointments.AsNoTracking()
                .Where(a => a.AppointmentDate >= seriesStart && a.AppointmentDate < rangeEnd)
                .GroupBy(a => a.AppointmentDate.Date)
                .Select(g => new { Day = g.Key, Count = g.Count() })
                .ToListAsync();
            var countLookup = countsByDay.ToDictionary(x => x.Day, x => x.Count);

            var daily = new List<DailyAppointmentCountDto>();
            for (var d = seriesStart; d <= today; d = d.AddDays(1))
            {
                var day = d.Date;
                countLookup.TryGetValue(day, out var cnt);
                daily.Add(new DailyAppointmentCountDto
                {
                    Date = day.ToString("yyyy-MM-dd"),
                    Label = day.ToString("dd/MM"),
                    Count = cnt
                });
            }

            var dto = new AdminDashboardStatsDto
            {
                PatientCount = patientCount,
                DoctorActiveCount = doctorActiveCount,
                AppointmentsToday = appointmentsToday,
                AppointmentsByStatus = byStatus,
                RevenueThisMonth = revenueThisMonth,
                AppointmentsLast14Days = daily
            };

            return Ok(dto);
        }
        /// <summary>
        /// Doanh thu theo filter: ngày / tháng / năm / khoa / loại hóa đơn
        /// </summary>
        [HttpGet("revenue")]
        public async Task<IActionResult> GetRevenue(
            [FromQuery] int? year,
            [FromQuery] int? month,
            [FromQuery] Guid? departmentId,
            [FromQuery] string? invoiceType)
        {
            var query = _context.Invoices
                .AsNoTracking()
                .Include(i => i.Appointment)
                    .ThenInclude(a => a.Doctor)
                        .ThenInclude(d => d.Department)
                .AsQueryable();

            // ===== FILTER THEO NĂM =====
            if (year.HasValue)
            {
                query = query.Where(i => i.CreatedAt.Year == year.Value);
            }

            // ===== FILTER THEO THÁNG =====
            if (month.HasValue)
            {
                query = query.Where(i => i.CreatedAt.Month == month.Value);
            }

            // ===== FILTER THEO LOẠI HÓA ĐƠN =====
            if (!string.IsNullOrEmpty(invoiceType))
            {
                query = query.Where(i => i.InvoiceType.ToString() == invoiceType);
            }

            // ===== FILTER THEO KHOA =====
            if (departmentId.HasValue)
            {
                query = query.Where(i =>
                    i.Appointment.Doctor != null &&
                    i.Appointment.Doctor.DepartmentId == departmentId.Value);
            }

            var data = await query.ToListAsync();

            // ===== TỔNG DOANH THU =====
            var totalRevenue = data.Sum(x => x.Amount);

            // ===== GROUP THEO NGÀY =====
            var byDay = data
                .GroupBy(x => x.CreatedAt.Date)
                .Select(g => new
                {
                    Label = g.Key.ToString("dd/MM"),
                    Value = g.Sum(x => x.Amount)
                })
                .OrderBy(x => x.Label)
                .ToList();

            // ===== GROUP THEO THÁNG =====
            var byMonth = data
                .GroupBy(x => new { x.CreatedAt.Year, x.CreatedAt.Month })
                .Select(g => new
                {
                    Label = $"{g.Key.Month}/{g.Key.Year}",
                    Value = g.Sum(x => x.Amount)
                })
                .ToList();

            // ===== GROUP THEO KHOA =====
            var byDepartment = data
                .GroupBy(x => x.Appointment.Doctor != null ? x.Appointment.Doctor.Department.Name : "Unknown")
                .Select(g => new
                {
                    Label = g.Key,
                    Value = g.Sum(x => x.Amount)
                })
                .ToList();

            // ===== GROUP THEO LOẠI HÓA ĐƠN =====
            var byType = data
                .GroupBy(x => x.InvoiceType.ToString())
                .Select(g => new
                {
                    Label = g.Key,
                    Value = g.Sum(x => x.Amount)
                })
                .ToList();

            return Ok(new
            {
                totalRevenue,
                byDay,
                byMonth,
                byDepartment,
                byType
            });
        }
    }

    public class AdminDashboardStatsDto
    {
        public int PatientCount { get; set; }
        public int DoctorActiveCount { get; set; }
        public int AppointmentsToday { get; set; }
        public Dictionary<string, int> AppointmentsByStatus { get; set; } = new();
        public decimal RevenueThisMonth { get; set; }
        public List<DailyAppointmentCountDto> AppointmentsLast14Days { get; set; } = new();
    }

    public class DailyAppointmentCountDto
    {
        public string Date { get; set; } = "";
        public string Label { get; set; } = "";
        public int Count { get; set; }
    }
}
