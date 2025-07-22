namespace Synapse_API.Models.Dto.LearningAnalysisDTOs
{
    public class TopicTrendDto
    {
        public int TopicId { get; set; }
        public string TopicName { get; set; } = string.Empty;
        public double CorrectRate { get; set; }
        public double StudyTime { get; set; }
        public double TrendScore { get; set; }
        public string Insight { get; set; } = string.Empty;
    }
}
