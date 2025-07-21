using Synapse_API.Models.Enums;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Synapse_API.Models.Entities
{
    public class LearningActivity
    {
        [Key]
        public int ActivityID { get; set; }

        [ForeignKey("User")]
        public int UserID { get; set; }

        [StringLength(50)]
        public ActivityType ActivityType { get; set; }

        public DateTime StartTime { get; set; }

        public DateTime EndTime { get; set; }

        public int Duration { get; set; } // Day

        public User User { get; set; }
    }
}