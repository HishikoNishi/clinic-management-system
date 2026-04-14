namespace ClinicManagement.Api.Dtos.Queues
{
    public class PatientQueueStatusDto
    {
        public string AppointmentCode { get; set; } = string.Empty;
        public string AppointmentStatus { get; set; } = string.Empty;
        public QueueItemDto? OwnQueue { get; set; }
        public QueueItemDto? CurrentCalling { get; set; }
        public int WaitingAhead { get; set; }
        public string? Message { get; set; }
    }
}
