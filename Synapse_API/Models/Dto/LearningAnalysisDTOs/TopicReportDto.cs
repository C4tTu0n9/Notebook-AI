namespace Synapse_API.Models.Dto.LearningAnalysisDTOs
{
    public class TopicReportDto
    {
        public int TopicId { get; set; }
        public string TopicName { get; set; }
        public string CourseName { get; set; }
        public double AverageTime { get; set; }
        public double AverageCorrectRate { get; set; }
        public double AverageTrendScore { get; set; }
        public int AttemptCount { get; set; }
    }
}
