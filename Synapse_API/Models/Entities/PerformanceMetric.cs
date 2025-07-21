using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Synapse_API.Models.Entities
{
    public class PerformanceMetric
    {
        [Key]
        public int MetricID { get; set; }

        [ForeignKey("User")]
        public int UserID { get; set; }

        [ForeignKey("Topic")]
        public int TopicID { get; set; }

        public decimal? AverageTime { get; set; }

        public decimal? CorrectRate { get; set; }

        public decimal? TrendScore { get; set; }

        public DateTime LastUpdated { get; set; } = DateTime.Now;

        public User User { get; set; }
        public Topic Topic { get; set; }
    }
}