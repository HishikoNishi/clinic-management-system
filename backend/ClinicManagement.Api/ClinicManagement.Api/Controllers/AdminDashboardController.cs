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
