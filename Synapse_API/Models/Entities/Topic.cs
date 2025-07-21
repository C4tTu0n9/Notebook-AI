using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Synapse_API.Models.Entities
{
    public class Topic
    {
        [Key]
        public int TopicID { get; set; }

        [ForeignKey("Course")]
        public int CourseID { get; set; }

        [Required]
        [StringLength(100)]
        public string TopicName { get; set; }

        public string? Description { get; set; }
        public string? DocumentUrl { get; set; }

        public Course Course { get; set; }
        public ICollection<PathTopic> PathTopics { get; set; }
        public ICollection<Quiz> Quizzes { get; set; }
        public ICollection<PerformanceMetric> PerformanceMetrics { get; set; }
    }
}