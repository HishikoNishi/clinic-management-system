using ClinicManagement.Api.Models;

namespace ClinicManagement.Api.Dtos.Appointments
{
    public class CheckInRequestDto
    {
        public Guid AppointmentId { get; set; }
        public Guid? DoctorId { get; set; }
        public decimal DepositAmount { get; set; }
        public PaymentMethod Method { get; set; } = PaymentMethod.cash;
    }
}
