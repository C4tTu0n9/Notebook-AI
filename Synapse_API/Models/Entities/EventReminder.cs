using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Synapse_API.Models.Entities
{
    public class EventReminder
    {
        [Key]
        public int ReminderID { get; set; }

        [ForeignKey("Event")]
        public int EventID { get; set; }

        public DateTime ReminderTime { get; set; }

        public bool IsSent { get; set; } = false;

        public Event Event { get; set; }
    }
}