namespace Synapse_API.Models.Dto.LearningAnalysisDTOs
{
    public class LearningReportDto
    {
        public string User { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
        public List<TopicReportDto> Topics { get; set; }
        public List<GoalDto> Goals { get; set; }
    }
}
