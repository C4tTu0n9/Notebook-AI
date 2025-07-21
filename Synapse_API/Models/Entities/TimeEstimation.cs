using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Synapse_API.Models.Entities
{
    public class TimeEstimation
    {
        [Key]
        public int EstimationID { get; set; }

        [ForeignKey("User")]
        public int UserID { get; set; }

        [StringLength(50)]
        public string TaskType { get; set; }

        public int? EstimatedTime { get; set; } // Day/Hour/Minute?

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public User User { get; set; }
    }
}