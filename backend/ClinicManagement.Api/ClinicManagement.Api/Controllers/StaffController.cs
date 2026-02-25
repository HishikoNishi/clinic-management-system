using ClinicManagement.Api.Data;
using ClinicManagement.Api.Dtos.Invoices;
using ClinicManagement.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Api.Controllers
{
    [Authorize(Roles = "Staff")]
    [ApiController]
    [Route("api/staff")]
    public class StaffController : ControllerBase
    {
        private readonly ClinicDbContext _context;

        public StaffController(ClinicDbContext context)
        {
            _context = context;
        }

        // ==============================
        // 1️⃣ Xác nhận lịch hẹn
        // ==============================
        [HttpPut("appointments/{id}/confirm")]
        public async Task<IActionResult> ConfirmAppointment(Guid id)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null)
                return NotFound("Không tìm thấy lịch hẹn");

            if (appointment.Status != AppointmentStatus.Pending)
                return BadRequest("Chỉ xác nhận lịch đang ở trạng thái Pending");

            appointment.Status = AppointmentStatus.Confirmed;
            await _context.SaveChangesAsync();

            return Ok("Xác nhận lịch hẹn thành công");
        }

        // ==============================
        // 2️⃣ Phân bác sĩ
        // ==============================
        [HttpPut("appointments/{id}/assign-doctor/{doctorId}")]
        public async Task<IActionResult> AssignDoctor(Guid id, Guid doctorId)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null)
                return NotFound("Không tìm thấy lịch hẹn");

            var doctor = await _context.Doctors.FindAsync(doctorId);
            if (doctor == null)
                return NotFound("Không tìm thấy bác sĩ");

            appointment.DoctorId = doctorId;
            appointment.Status = AppointmentStatus.Assigned;

            await _context.SaveChangesAsync();
            return Ok("Phân bác sĩ thành công");
        }

        // ==============================
        // 3️⃣ Check-in bệnh nhân
        // ==============================
        [HttpPut("appointments/{id}/check-in")]
        public async Task<IActionResult> CheckIn(Guid id)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null)
                return NotFound("Không tìm thấy lịch hẹn");

            if (appointment.Status != AppointmentStatus.Assigned)
                return BadRequest("Lịch phải được phân bác sĩ trước");

            appointment.Status = AppointmentStatus.CheckedIn;
            await _context.SaveChangesAsync();

            return Ok("Bệnh nhân đã check-in");
        }

        // ==============================
        // 4️⃣ Tạo hoá đơn
        // ==============================
        [HttpPost("invoices")]
        public async Task<IActionResult> CreateInvoice([FromBody] TaoHoaDonDto dto)
        {
            var appointment = await _context.Appointments.FindAsync(dto.MaLichHen);
            if (appointment == null)
                return NotFound("Không tìm thấy lịch hẹn");

            var invoice = new HoaDon
            {
                Id = Guid.NewGuid(),
                MaLichHen = dto.MaLichHen,
                SoTien = dto.SoTien,
                NgayTao = DateTime.UtcNow,
                DaThanhToan = false
            };

            _context.Invoices.Add(invoice);
            await _context.SaveChangesAsync();

            return Ok(invoice);
        }

        // ==============================
        // 5️⃣ Thanh toán hoá đơn
        // ==============================
        [HttpPut("invoices/{id}/pay")]
        public async Task<IActionResult> PayInvoice(Guid id)
        {
            var invoice = await _context.Invoices.FindAsync(id);
            if (invoice == null)
                return NotFound("Không tìm thấy hoá đơn");

            if (invoice.DaThanhToan)
                return BadRequest("Hoá đơn đã thanh toán");

            invoice.DaThanhToan = true;
            invoice.NgayThanhToan = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return Ok("Thanh toán thành công");
        }

        // ==============================
        // 6️⃣ In phiếu khám
        // ==============================
        [HttpGet("appointments/{id}/print")]
        public async Task<IActionResult> PrintTicket(Guid id)
        {
            var appointment = await _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Doctor)
                .ThenInclude(d => d.User)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (appointment == null)
                return NotFound("Không tìm thấy lịch hẹn");

            return Ok(new
            {
                appointment.AppointmentCode,
                appointment.AppointmentDate,
                appointment.AppointmentTime,
                Patient = appointment.Patient?.FullName,
                Doctor = appointment.Doctor?.User?.Username,
                appointment.Reason
            });
        }
    }
}