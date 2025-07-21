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


        public async Task<List<UserQuizAttempt>> GetAllUserQuizAttemptsByUserIdAsync(int userId, int month, int year)
        {
            return await _analyticsRepository.GetAllUserQuizAttemptsByUserIdAsync(userId, month, year);
        }


        public async Task<List<PerformanceMetric>> GetWeakTopicsAsync(int userId)
        {
            var allMetrics = await _analyticsRepository.GetWeakTopicsByUserIdAsync(userId);

            var weakTopics = allMetrics
                .GroupBy(m => m.TopicID)
                .Where(g => g.Average(m => m.CorrectRate ?? 0) < 50)
                .SelectMany(g => g)
                .ToList();

            return weakTopics;
        }

        public async Task<List<TopicTrendDto>> GetLearningTrendsGroupedAsync(int userId)
        {
            return await _analyticsRepository.GetLearningTrendsByTopicAsync(userId);

        }


        public async Task<List<EnhancedLearningReportDto>> GenerateLearningReportAsync(int userId, int month, int year)
        {
            return await _analyticsRepository.GenerateEnhancedLearningReportAsync(userId, month, year);
        }

       
        public async Task<List<GoalComparisonDto>> ComparePerformanceToGoalsAsync(int userId, int month, int year)
        {
            var goals = await _analyticsRepository.GetGoalsByUserIdAsync(userId);
            var quizAttempts = await _analyticsRepository.GetAllUserQuizAttemptsByUserIdAsync(userId, month, year);

            // Debug: Log tất cả attempts
            Console.WriteLine($"Tổng số attempts: {quizAttempts.Count}");
            foreach (var attempt in quizAttempts)
            {
                Console.WriteLine($"AttemptID: {attempt.AttemptID}, QuizID: {attempt.QuizID}, " +
                                 $"TopicID: {attempt.Quiz?.TopicID}, Score: {attempt.Score}, " +
                                 $"Date: {attempt.AttemptDate}");
            }

            var result = new List<GoalComparisonDto>();

            foreach (var goal in goals)
            {
                Console.WriteLine($"\nĐang xử lý GoalID: {goal.GoalID}, TopicID: {goal.TopicID}, TargetScore: {goal.TargetScore}");

                var relevantAttempts = quizAttempts
                    .Where(q => q.Quiz != null &&
                               q.Quiz.TopicID == goal.TopicID && // Quan trọng nhất
                               q.AttemptDate <= goal.TargetDate)
                    .ToList();

                // Debug: Log các attempts liên quan
                Console.WriteLine($"Tìm thấy {relevantAttempts.Count} attempts liên quan:");
                foreach (var a in relevantAttempts)
                {
                    Console.WriteLine($"- AttemptID: {a.AttemptID}, Score: {a.Score}, Date: {a.AttemptDate}");
                }

                var averageScore = relevantAttempts.Any()
                    ? relevantAttempts.Average(a => a.Score)
                    : 0;

                Console.WriteLine($"Điểm trung bình tính được: {averageScore}");

                string status;
                if (DateTime.UtcNow > goal.TargetDate)
                {
                    status = averageScore >= goal.TargetScore ? "Achieved (Late)" : "Behind Schedule";
                }
                else
                {
                    status = averageScore >= goal.TargetScore ? "Achieved Early" : "In Progress";
                }

                result.Add(new GoalComparisonDto
                {
                    GoalId = goal.GoalID,
                    Description = goal.GoalDescription,
                    TargetDate = goal.TargetDate ?? DateTime.MinValue,
                    TargetScore = (double)goal.TargetScore,
                    AverageScore = Math.Round((double)averageScore, 2),
                    Status = status,
                    TopicName = goal.Topic?.TopicName ?? "Unknown",
                });
            }

            return result;
        }

        public async Task<string> BuildLearningSuggestionPromptAsync(int userId, int month, int year)
        {
            var metrics = await _analyticsRepository.GenerateEnhancedLearningReportAsync(userId, month, year);

            if (!metrics.Any())
                return "No data available for this student during the selected period. Please analyze manually.";

            var sb = new StringBuilder();

            sb.AppendLine("Each suggestion must be:");
            sb.AppendLine("- Actionable (tell the student exactly what to practice)");
            sb.AppendLine("- Focus only on quiz results — do NOT recommend watching videos");
            sb.AppendLine("- Mention what kind of questions to review or retry");
            sb.AppendLine("- Limit each suggestion to 2 lines");
            sb.AppendLine("- Group suggestions by topic name");
            sb.AppendLine("- Use clear bullet points, up to 3 per topic\n");

            foreach (var m in metrics)
            {
                sb.AppendLine($"Topic: {m.TopicName ?? "Unknown"}");
                sb.AppendLine($"- AverageScore: {m.Performance.AverageScore:F1}");
                sb.AppendLine($"- HighestScore: {m.Performance.HighestScore:F1}");
                sb.AppendLine($"- TargetScore: {(m.Goal?.TargetScore ?? 0):F1}");
                sb.AppendLine($"- GoalDescription: {m.Goal?.Description ?? "No goal set"}");

                // Optional: You can include recent quiz titles or feedback if useful
                var recentAttempts = m.Attempts.Take(2).ToList();
                foreach (var attempt in recentAttempts)
                {
                    sb.AppendLine($"  RecentQuiz: {attempt.QuizTitle}, Score: {attempt.Score:F1}, Feedback: {attempt.Feedback}");
                }

                sb.AppendLine(); // Blank line between topics
            }

            sb.AppendLine("Now provide clear, practical suggestions for improvement under each topic.");
            return sb.ToString();
        }



        

    }
}
