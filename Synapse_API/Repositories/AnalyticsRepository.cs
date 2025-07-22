using Microsoft.EntityFrameworkCore;
using Synapse_API.Data;
using Synapse_API.Models.Dto.LearningAnalysisDTOs;
using Synapse_API.Models.Dto.LearningReportDto;
using Synapse_API.Models.Entities;

namespace Synapse_API.Repositories
{
    public class AnalyticsRepository
    {
        private readonly SynapseDbContext _context;

        public AnalyticsRepository(SynapseDbContext context)
        {
            _context = context;
        }

        public async Task<PerformanceMetric> CalculateMetricForUserAndTopicAsync(int userId, int topicId)
        {
            var records = await _context.PerformanceMetrics
                .Where(m => m.UserID == userId && m.TopicID == topicId)
                .ToListAsync();

            if (records.Count == 0)
                throw new Exception("No performance records found.");

            return new PerformanceMetric
            {
                UserID = userId,
                TopicID = topicId,
                AverageTime = records.Average(m => m.AverageTime),
                CorrectRate = records.Average(m => m.CorrectRate),
                TrendScore = records.Average(m => m.TrendScore),
                LastUpdated = DateTime.Now
            };
        }

        public async Task<List<PerformanceMetric>> GetAllMetricsByUserIdAsync(int userId,int month, int year)
        {
            return await _context.PerformanceMetrics
                                     .Where(m => m.UserID == userId &&
                                     m.LastUpdated.Month == month &&
                                     m.LastUpdated.Year == year)
                                 .ToListAsync();
        }

        public async Task<List<UserQuizAttemptDto>> GetAllUserQuizAttemptsByUserIdAsync(int userId, int topicId, int month, int year)
        {
            // Lấy tất cả các UserQuizAttempts theo userId
            return await _context.UserQuizAttempts
                .Include(a => a.Quiz)
                .ThenInclude(q => q.Topic)
                .Where(a => a.UserID == userId &&
                            a.AttemptDate.Month == month &&
                            a.AttemptDate.Year == year &&
                            a.Quiz.TopicID == topicId)
                .OrderBy(a => a.AttemptDate)
                .Select(a => new UserQuizAttemptDto
                {
                    AttemptID = a.AttemptID,
                    QuizID = a.QuizID,
                    QuizTitle = a.Quiz.QuizTitle,
                    Score = (double)a.Score,
                    AttemptDate = a.AttemptDate,
                    Feedback = a.Feedback
                })
                .ToListAsync();
        }

        public async Task<List<WeakTopicDto>> GetWeakTopicsByUserIdAsync(int userId)
        {
            // Lấy tất cả các PerformanceMetrics theo userId
            return await _context.PerformanceMetrics
                .Where(m => m.UserID == userId && m.CorrectRate < 50)
                .Include(m => m.Topic)
                    .ThenInclude(t => t.Course)
                .Select(m => new WeakTopicDto
                {
                    TopicName = m.Topic.TopicName,
                    CourseName =m.Topic.Course.CourseName,
                    CorrectRate = (double)m.CorrectRate,
                })
                .ToListAsync();
        }

        public async Task<List<TopicTrendDto>> GetLearningTrendsByTopicAsync(int userId)
        {
            // Lấy tất cả các PerformanceMetrics theo userId
            var metrics = await _context.PerformanceMetrics
                   .Include(m => m.Topic)
                   .Where(m => m.UserID == userId)
                   .ToListAsync();

            var result = new List<TopicTrendDto>();

            foreach (var metric in metrics)
            {
                string insight = "";

                if (metric.TrendScore >= 8 && metric.CorrectRate >= 80)
                    insight = $"Đang cải thiện tốt môn {metric.Topic.TopicName} 🎯";
                else if (metric.TrendScore < 6 || metric.CorrectRate < 60)
                    insight = $"Hiệu suất môn {metric.Topic.TopicName} đang giảm, nên ôn tập thêm ⚠️";
                else
                    insight = $"Hiệu suất môn {metric.Topic.TopicName} ổn định.";

                result.Add(new TopicTrendDto
                {
                    TopicId = metric.TopicID,
                    TopicName = metric.Topic.TopicName,
                    CorrectRate = (double)metric.CorrectRate,
                    TrendScore = (double)metric.TrendScore,
                    Insight = insight
                });
            }
            return result;
        }

        public async Task<List<EnhancedLearningReportDto>> GenerateEnhancedLearningReportAsync(int userId, int month, int year)
        {
            // Lấy danh sách các Topic mà học sinh đã làm quiz
            var topicIds = await _context.UserQuizAttempts
                .Where(a => a.UserID == userId &&
                            a.AttemptDate.Month == month &&
                            a.AttemptDate.Year == year)
                .Select(a => a.Quiz.TopicID)
                .Distinct()
                .ToListAsync();

            // Lấy UserName
            var userName = await _context.Users
                .Where(u => u.UserID == userId)
                .Select(u => u.FullName)
                .FirstOrDefaultAsync();

            var reports = new List<EnhancedLearningReportDto>();

            foreach (var topicId in topicIds)
            {
                // Lấy TopicName
                var topicName = await _context.Topics
                    .Where(t => t.TopicID == topicId)
                    .Select(t => t.TopicName)
                    .FirstOrDefaultAsync();

                // Lấy các lần làm quiz theo chủ đề đó trong tháng & năm
                var attempts = await _context.UserQuizAttempts
                    .Where(a => a.UserID == userId &&
                                a.AttemptDate.Month == month &&
                                a.AttemptDate.Year == year &&
                                a.Quiz.TopicID == topicId)
                    .Select(a => new AttemptDto
                    {
                        AttemptID = a.AttemptID,
                        QuizID = a.QuizID,
                        QuizTitle = a.Quiz.QuizTitle,
                        Score = (double)(a.Score ?? 0),
                        AttemptDate = a.AttemptDate,
                        Feedback = a.Feedback
                    })
                    .ToListAsync();

                if (!attempts.Any())
                    continue;

                // Tính điểm trung bình và điểm cao nhất
                var avgScore = attempts.Average(a => a.Score);
                var highScore = attempts.Max(a => a.Score);

                //  Tạo đối tượng báo cáo
                var report = new EnhancedLearningReportDto
                {
                    UserId = userId,
                    UserName = userName,
                    TopicId = topicId,
                    TopicName = topicName,
                    Month = month,
                    Year = year,
                    Performance = new PerformanceDto
                    {
                        AverageScore = avgScore,
                        HighestScore = highScore
                    },
                    Attempts = attempts
                };

                reports.Add(report);
            }

            return reports;
        }


    }
}
