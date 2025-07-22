using Synapse_API.Models.Dto.LearningAnalysisDTOs;

namespace Synapse_API.Models.Dto.LearningReportDto
{
    public class EnhancedLearningReportDto
    {
        public int UserId { get; set; }
        public string UserName { get; set; }          
        public int TopicId { get; set; }
        public string TopicName { get; set; }          
        public int Month { get; set; }
        public int Year { get; set; }
        public PerformanceDto Performance { get; set; }
        public List<AttemptDto> Attempts { get; set; }

    }
}
