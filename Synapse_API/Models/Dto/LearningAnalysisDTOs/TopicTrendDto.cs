namespace Synapse_API.Models.Dto.LearningAnalysisDTOs
{
    public class TopicTrendDto
    {
        public int TopicId { get; set; }
        public string TopicName { get; set; } = string.Empty;
        public double AverageCorrectRate { get; set; }
        public double AverageStudyTime { get; set; }
        public double AverageTrendScore { get; set; }
        public string Insight { get; set; } = string.Empty;
    }
}
