namespace Synapse_API.Models.Dto.LearningAnalysisDTOs
{
    public class GoalComparisonDto
    {
        public int GoalId { get; set; }
        public string Description { get; set; }
        public DateTime TargetDate { get; set; }
        public double TargetScore { get; set; }
        public double AverageScore { get; set; }
        public string Status { get; set; }
        public string TopicName { get; set; }

    }
}
