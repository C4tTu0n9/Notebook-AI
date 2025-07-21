using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Synapse_API.Models.Entities
{
    public class LearningPath
    {

        [Key]
        public int PathID { get; set; }

        [ForeignKey("User")]
        public int UserID { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        public User User { get; set; }
        public ICollection<PathTopic> PathTopics { get; set; }
    }
}