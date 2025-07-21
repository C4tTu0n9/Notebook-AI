using System.ComponentModel.DataAnnotations.Schema;

namespace Synapse_API.Models.Dto.ReminderDTOs
{
    public class ReminderDto
    {
        public int ReminderID { get; set; }

        public int EventID { get; set; }

        public DateTime ReminderTime { get; set; }

        public bool IsSent { get; set; } = false;
    }
}
