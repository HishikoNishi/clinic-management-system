namespace ClinicManagement.Api.Dtos.Queues
{
    public class RoomQueueStateDto
    {
        public Guid RoomId { get; set; }
        public string RoomCode { get; set; } = string.Empty;
        public string RoomName { get; set; } = string.Empty;
        public Guid DepartmentId { get; set; }
        public string DepartmentName { get; set; } = string.Empty;
        public int WaitingCount { get; set; }
        public int InProgressCount { get; set; }
        public QueueItemDto? CurrentCalling { get; set; }
        public List<QueueItemDto> Items { get; set; } = new();
    }
}
