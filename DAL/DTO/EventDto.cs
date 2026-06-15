namespace DAL.DTO
{
    public class EventDto
    {
        public int EventId { get; set; }
        public string? EventName { get; set; }
        public string? EventType { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? GameName { get; set; }
    }
}
