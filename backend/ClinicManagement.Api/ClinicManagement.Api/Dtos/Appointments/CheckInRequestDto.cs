using ClinicManagement.Api.Models;

namespace ClinicManagement.Api.Dtos.Appointments
{
    public class CheckInRequestDto
    {
        public Guid AppointmentId { get; set; }
        public Guid? DoctorId { get; set; }
        public Guid? RoomId { get; set; }
        public decimal DepositAmount { get; set; }
        public PaymentMethod Method { get; set; } = PaymentMethod.cash;
        public string? CitizenId { get; set; }
        public string? InsuranceCardNumber { get; set; }
        // Xác nhận bảo hiểm trước khi thu tạm ứng
        public string? InsuranceCode { get; set; }
        // Giá trị 0-1 đã được validate ở client
        public decimal? InsuranceCoverPercent { get; set; }

        // Nhập viện (mới thu tạm ứng)
        public bool IsInpatient { get; set; } = false;
    }
}
