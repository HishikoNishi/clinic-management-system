namespace ClinicManagement.Api.Dtos.Queues
{
    public class QueueCheckInDto
    {
        public Guid AppointmentId { get; set; }
        public Guid? RoomId { get; set; }
    }
}
