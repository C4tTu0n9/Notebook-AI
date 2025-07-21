using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Synapse_API.Models.Entities
{
    public class Goal
    {
        [Key]
        public int GoalID { get; set; }

        [ForeignKey("User")]
        public int UserID { get; set; }

        [ForeignKey("Topic")]
        public int TopicID { get; set; }

        [Required]
        [StringLength(255)]
        public string GoalDescription { get; set; }

        public DateTime? TargetDate { get; set; }

        public bool IsAchieved { get; set; } = false;

        public int? TargetScore { get; set; }

        public User User { get; set; }

        public Topic Topic { get; set; }
    }
}