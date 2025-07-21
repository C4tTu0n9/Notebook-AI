namespace Synapse_API.Models.Dto.LearningReportDto
{
    public class TopicPerformanceDto
    {
        public int TopicId { get; set; }
        public string TopicName { get; set; }
        public int QuizzesTaken { get; set; }
        public double AverageScore { get; set; }
        public string AccuracyTrend { get; set; }
        public List<string> WeakAreas { get; set; }
    }
}
