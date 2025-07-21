using Synapse_API.Models.Enums;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Synapse_API.Models.Entities
{
    public class Event
    {
        [Key]
        public int EventID { get; set; }

        [ForeignKey("User")]
        public int UserID { get; set; }

        [StringLength(50)]
        public EventType EventType { get; set; }

        [Required]
        [StringLength(100)]
        public string Title { get; set; }

        public string? Description { get; set; }

        public DateTime StartTime { get; set; }

        public DateTime EndTime { get; set; }

        public int? Priority { get; set; }

        public bool IsCompleted { get; set; } = false;

        [ForeignKey("Course")]
        public int? CourseID { get; set; }
        
        [ForeignKey("ParentEvent")]
        public int? ParentEventID { get; set; }

        public User User { get; set; }
        public Course Course { get; set; }
        public Event ParentEvent { get; set; }
        public ICollection<Event> ChildEvents { get; set; }
        public ICollection<EventReminder> EventReminders { get; set; }
    }
}