using ClinicManagement.Api.Data;
using ClinicManagement.Api.Dtos.Invoices;
using ClinicManagement.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NhanVienController : ControllerBase
    {
        private readonly ClinicDbContext _context;

        public NhanVienController(ClinicDbContext context)
        {
            _context = context;
        }

        // ✅ Xác nhận lịch hẹn
        [HttpPut("lich-hen/{id}/xac-nhan")]
        public async Task<IActionResult> XacNhanLichHen(Guid id)
        {
            var lichHen = await _context.Appointments.FindAsync(id);
            if (lichHen == null) return NotFound("Không tìm thấy lịch hẹn");

            lichHen.Status = AppointmentStatus.Confirmed;
            await _context.SaveChangesAsync();
            return Ok(lichHen);
        }

        // ✅ Phân bác sĩ cho lịch khám
        [HttpPut("lich-hen/{id}/phan-bac-si/{doctorId}")]
        public async Task<IActionResult> PhanBacSi(Guid id, Guid doctorId)
        {
            var lichHen = await _context.Appointments.FindAsync(id);
            if (lichHen == null) return NotFound("Không tìm thấy lịch hẹn");

            lichHen.DoctorId = doctorId;
            await _context.SaveChangesAsync();
            return Ok(lichHen);
        }

        // ✅ Check-in bệnh nhân
        [HttpPut("lich-hen/{id}/check-in")]
        public async Task<IActionResult> CheckInBenhNhan(Guid id)
        {
            var lichHen = await _context.Appointments.FindAsync(id);
            if (lichHen == null) return NotFound("Không tìm thấy lịch hẹn");

            lichHen.Status = AppointmentStatus.Confirmed; // hoặc thêm trạng thái riêng "CheckedIn"
            await _context.SaveChangesAsync();
            return Ok(lichHen);
        }

        // ✅ Tạo hóa đơn
        [HttpPost("hoa-don")]
        public async Task<IActionResult> TaoHoaDon([FromBody] TaoHoaDonDto dto)
        {
            var lichHen = await _context.Appointments.FindAsync(dto.MaLichHen);
            if (lichHen == null) return NotFound("Không tìm thấy lịch hẹn");

            var hoaDon = new HoaDon
            {
                MaLichHen = dto.MaLichHen,
                SoTien = dto.SoTien,
                NgayTao = DateTime.UtcNow,
                DaThanhToan = false
            };

            _context.Invoices.Add(hoaDon);
            await _context.SaveChangesAsync();
            return Ok(hoaDon);
        }

        // ✅ Thanh toán hóa đơn
        [HttpPut("hoa-don/thanh-toan")]
        public async Task<IActionResult> ThanhToanHoaDon([FromBody] ThanhToanHoaDonDto dto)
        {
            var hoaDon = await _context.Invoices.FindAsync(dto.MaHoaDon);
            if (hoaDon == null) return NotFound("Không tìm thấy hóa đơn");

            if (hoaDon.DaThanhToan) return BadRequest("Hóa đơn đã được thanh toán");

            hoaDon.DaThanhToan = true;
            hoaDon.NgayThanhToan = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return Ok(hoaDon);
        }

        // ✅ In phiếu khám
        [HttpGet("lich-hen/{id}/in-phieu")]
        public async Task<IActionResult> InPhieuKham(Guid id)
        {
            var lichHen = await _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Doctor)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (lichHen == null) return NotFound("Không tìm thấy lịch hẹn");

            return Ok(new
            {
                lichHen.AppointmentCode,
                lichHen.AppointmentDate,
                lichHen.AppointmentTime,
                BenhNhan = lichHen.Patient?.FullName,
                BacSi = lichHen.Doctor?.User.FullName,
                LyDo = lichHen.Reason
            });
        }
    }
}
