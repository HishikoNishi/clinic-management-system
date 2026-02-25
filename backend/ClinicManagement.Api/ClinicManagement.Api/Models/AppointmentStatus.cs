using System;
namespace ClinicManagement.Api.Models
{
    public enum AppointmentStatus
    {
        Pending = 0,      // Guest vừa đặt
        Confirmed = 1,    // Staff xác nhận lịch
        Assigned = 2,     // Đã phân bác sĩ
        CheckedIn = 3,    // Bệnh nhân đã tới
        Completed = 4,    // Khám xong
        Cancelled = 5     // Huỷ lịch
    }
}

