namespace Synapse_API.Models.Dto.EventDTOs
{
    public class EventDto
    {
        public int EventID { get; set; }
        public int UserID { get; set; }
        public string EventType { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public int? Priority { get; set; }
        public bool IsCompleted { get; set; }
        public int? CourseID { get; set; }
        public int? ParentEventID { get; set; }
        public List<EventDto> ChildEvents { get; set; } = new List<EventDto>(); 
    }

}
