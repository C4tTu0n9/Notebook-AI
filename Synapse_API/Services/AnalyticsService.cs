using Synapse_API.Models.Dto.LearningAnalysisDTOs;
using Synapse_API.Models.Dto.LearningReportDto;
using Synapse_API.Models.Entities;
using Synapse_API.Repositories;
using System.Text;


namespace Synapse_API.Services
{
    public class AnalyticsService
    {
        private readonly AnalyticsRepository _analyticsRepository;

        public AnalyticsService(AnalyticsRepository analyticsRepository)
        {
            _analyticsRepository = analyticsRepository;
        }

        public async Task<PerformanceMetric> CalculateAndStoreLearningMetricsByUserAndTopicAsync(int userId, int topicId)
        {
            var metric = await _analyticsRepository.CalculateMetricForUserAndTopicAsync(userId, topicId);
            return metric;
        }


        public async Task<List<PerformanceMetric>> GetAllMetricsByUserIdAsync(int userId,int month,int year)
        {
            return await _analyticsRepository.GetAllMetricsByUserIdAsync(userId, month, year);
        }


        public async Task<List<UserQuizAttemptDto>> GetAllUserQuizAttemptsByUserIdAsync(int userId, int topicId, int month, int year)
        {
            return await _analyticsRepository.GetAllUserQuizAttemptsByUserIdAsync(userId, topicId, month, year);
        }


        public async Task<List<WeakTopicDto>> GetWeakTopicsAsync(int userId)
        {
            return await _analyticsRepository.GetWeakTopicsByUserIdAsync(userId);
        }

        public async Task<List<TopicTrendDto>> GetLearningTrendsGroupedAsync(int userId)
        {
            return await _analyticsRepository.GetLearningTrendsByTopicAsync(userId);

        }

        public async Task<List<EnhancedLearningReportDto>> GenerateLearningReportAsync(int userId, int month, int year)
        {
            return await _analyticsRepository.GenerateEnhancedLearningReportAsync(userId, month, year);
        }

       

        public async Task<string> BuildLearningSuggestionPromptAsync(int userId, int month, int year)
        {
            var metrics = await _analyticsRepository.GenerateEnhancedLearningReportAsync(userId, month, year);
           
            if (!metrics.Any())
                return "No data available for this student during the selected period. Please analyze manually.";

            var sb = new StringBuilder();

            foreach (var line in SuggestionGuidelines)
            {
                sb.AppendLine(line);
            }

            foreach (var m in metrics)
            {
                sb.AppendLine($"Topic: {m.TopicName ?? "Unknown"}");
                sb.AppendLine($"- AverageScore: {m.Performance.AverageScore:F1}");
                sb.AppendLine($"- HighestScore: {m.Performance.HighestScore:F1}");
            
                var recentAttempts = m.Attempts.Take(2).ToList();
                foreach (var attempt in recentAttempts)
                {
                    sb.AppendLine($"  RecentQuiz: {attempt.QuizTitle}, Score: {attempt.Score:F1}, Feedback: {attempt.Feedback}");
                }

                sb.AppendLine(); 
            }
            sb.AppendLine("Now provide clear, practical suggestions for improvement under each topic.");
            return sb.ToString();
        }

        private static readonly string[] SuggestionGuidelines = new[]
        {
            "Each suggestion must be:",
            "- Actionable (tell the student exactly what to practice)",
            "- Focus only on quiz results — do NOT recommend watching videos",
            "- Mention what kind of questions to review or retry",
            "- Limit each suggestion to 2 lines",
            "- Group suggestions by topic name",
            "- Use clear bullet points, up to 3 per topic",
            ""
        };



    }
}
