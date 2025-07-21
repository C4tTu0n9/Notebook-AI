using Synapse_API.Models.Enums;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Synapse_API.Models.Entities
{
    public class PathTopic
    {
        [Key]
        public int PathTopicID { get; set; }

        [ForeignKey("LearningPath")]
        public int PathID { get; set; }

        [ForeignKey("Topic")]
        public int TopicID { get; set; }

        public int Order { get; set; }

        [StringLength(50)]
        public PathTopicStatus Status { get; set; } = PathTopicStatus.NotStarted;

        public LearningPath LearningPath { get; set; }
        public Topic Topic { get; set; }
    }
}