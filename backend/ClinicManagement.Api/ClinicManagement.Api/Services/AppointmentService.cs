using ClinicManagement.Api.Data;
using ClinicManagement.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Api.Services
{
    public class AppointmentService
    {
        private readonly ClinicDbContext _context;

        // ✅ THÊM
        private readonly EmailService _emailService;

        public AppointmentService(ClinicDbContext context, EmailService emailService)
        {
            _context = context;

            // ✅ THÊM
            _emailService = emailService;
        }

        public async Task MarkNoShowAppointments()
        {
            Console.WriteLine("🔥 Background đang chạy...");
            var now = DateTime.Now;

            var appointments = await _context.Appointments
                .Include(a => a.Patient) // ✅ THÊM
                .Where(a =>
                    (a.Status == AppointmentStatus.Pending ||   
                     a.Status == AppointmentStatus.Confirmed)
                    && a.CheckedInAt == null
                )
                .ToListAsync();

            foreach (var appt in appointments)
            {
                var appointmentDateTime = appt.AppointmentDate.Date
                    .Add(appt.AppointmentTime);

                if (appointmentDateTime.AddMinutes(30) < now)
                {
                    // ✅ CHỐNG SPAM MAIL (chỉ xử lý khi chưa phải NoShow)
                    if (appt.Status != AppointmentStatus.NoShow)
                    {
                        appt.Status = AppointmentStatus.NoShow;

                        // ✅ GỬI EMAIL
                        try
                        {
                            if (!string.IsNullOrWhiteSpace(appt.Patient?.Email))
                            {
                                await _emailService.SendAsync(
                                    appt.Patient.Email,
                                    "Thông báo lịch khám bị hủy",
                                    $@"
                                    Xin chào {appt.Patient.FullName},<br/><br/>
                                    Lịch khám của bạn đã bị hủy do quá giờ.<br/>
                                    <b>Mã khám:</b> {appt.AppointmentCode}<br/>
                                    <b>Ngày:</b> {appt.AppointmentDate:dd/MM/yyyy}<br/>
                                    <b>Giờ:</b> {appt.AppointmentTime}<br/><br/>
                                    Vui lòng đặt lại lịch nếu cần.<br/><br/>
                                    Trân trọng!
                                    "
                                );
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Send email failed: " + ex.Message);
                        }
                    }
                }
            }

            await _context.SaveChangesAsync();
        }
    }
}