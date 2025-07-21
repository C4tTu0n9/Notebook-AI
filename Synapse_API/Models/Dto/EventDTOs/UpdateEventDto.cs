using System.ComponentModel.DataAnnotations;

namespace Synapse_API.Models.Dto.EventDTOs
{
    public class UpdateEventDto
    {
        [Required]
        public int EventID { get; set; }

        [Required]
        public string EventType { get; set; }

        [Required]
        [StringLength(100)]
        public string Title { get; set; }

        public string Description { get; set; }

        [Required]
        public DateTime StartTime { get; set; }

        [Required]
        public DateTime EndTime { get; set; }

        public int? Priority { get; set; }
        public bool IsCompleted { get; set; }
        public int? CourseID { get; set; }
        public int? ParentEventID { get; set; }
    }

}
