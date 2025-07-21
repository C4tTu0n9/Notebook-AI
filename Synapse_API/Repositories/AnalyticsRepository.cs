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

        public async Task<List<UserQuizAttempt>> GetAllUserQuizAttemptsByUserIdAsync(int userId, int month, int year)
        {
            return await _context.UserQuizAttempts
                    .Include(m => m.Quiz)
                    .ThenInclude(q => q.Topic) // Load cả thông tin Topic
                    .Where(m => m.UserID == userId &&
                               m.AttemptDate.Month == month &&
                               m.AttemptDate.Year == year)
                    .ToListAsync();
        }



        public async Task<List<PerformanceMetric>> GetWeakTopicsByUserIdAsync(int userId)
        {
            return await _context.PerformanceMetrics
                .Where(m => m.UserID == userId)
                .Include(m => m.Topic)
                    .ThenInclude(t => t.Course)
                .ToListAsync();
        }


        public async Task<List<object>> GetLearningTrendsGroupedAsync(int userId)
        {
            var metrics = await _context.PerformanceMetrics
                .Include(m => m.Topic)
                .Where(m => m.UserID == userId)
                .ToListAsync();

            var result = metrics
                .GroupBy(m => new { m.TopicID, m.Topic.TopicName })
                .Select(g => new
                {
                    TopicId = g.Key.TopicID,
                    TopicName = g.Key.TopicName,
                    Trends = g.GroupBy(x => new { x.LastUpdated.Year, x.LastUpdated.Month })
                        .Select(t => new
                        {
                            Year = t.Key.Year,
                            Month = t.Key.Month,
                            AverageTime = Math.Round(t.Average(m => m.AverageTime ?? 0), 2),
                            AverageCorrectRate = Math.Round(t.Average(m => m.CorrectRate ?? 0), 2),
                            AverageTrendScore = Math.Round(t.Average(m => m.TrendScore ?? 0), 2),
                            RecordCount = t.Count()
                        })
                        .OrderBy(t => t.Year)
                        .ThenBy(t => t.Month)
                        .ToList()
                })
                .ToList<object>();

            return result;
        }


        public async Task<List<PerformanceMetric>> GetMetricsByUserAndMonthAsync(int userId, int month, int year)
        {
            return await _context.PerformanceMetrics
                .Where(m => m.UserID == userId &&
                            m.LastUpdated.Month == month &&
                            m.LastUpdated.Year == year)
                .Include(m => m.Topic)
                    .ThenInclude(t => t.Course)
                .ToListAsync();
        }

        public async Task<List<Goal>> GetGoalsByUserAsync(int userId, int month, int year)
        {
            return await _context.Goals
                .Where(g => g.UserID == userId
                        && g.TargetDate.Value.Month == month
                        && g.TargetDate.Value.Year == year)
                .ToListAsync();
        }
        public async Task<List<Goal>> GetGoalsByUserIdAsync(int userId)
        {
            return await _context.Goals
                .Where(g => g.UserID == userId)
                .Include(g => g.Topic)
                .ToListAsync();
        }


        public async Task<List<PerformanceMetric>> GetPerformanceMetricsByUserAsync(int userId)
        {
            return await _context.PerformanceMetrics
                .Where(m => m.UserID == userId)
                .Include(m => m.Topic)
                .ToListAsync();
        }

        public async Task<string> GetUserNameByIdAsync(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            return user?.FullName ?? $"User {userId}";
        }



        public async Task<List<TopicTrendDto>> GetLearningTrendsByTopicAsync(int userId)
        {
            var metrics = await _context.PerformanceMetrics
                   .Include(m => m.Topic)
                   .Where(m => m.UserID == userId)
                   .ToListAsync();

            var result = new List<TopicTrendDto>();

            var grouped = metrics.GroupBy(m => new { m.Topic.TopicID, m.Topic.TopicName });

            foreach (var group in grouped)
            {
                var avgCorrect = group.Average(m => m.CorrectRate ?? 0);
                var avgTime = group.Average(m => m.AverageTime ?? 0);
                var avgTrend = group.Average(m => m.TrendScore ?? 0);

                string insight = "";

                if (avgTrend >= 8 && avgCorrect >= 80)
                    insight = $"Đang cải thiện tốt môn {group.Key.TopicName} 🎯";
                else if (avgTrend < 6 || avgCorrect < 60)
                    insight = $"Hiệu suất môn {group.Key.TopicName} đang giảm, nên ôn tập thêm ⚠️";
                else
                    insight = $"Hiệu suất môn {group.Key.TopicName} ổn định.";

                result.Add(new TopicTrendDto
                {
                    TopicId = group.Key.TopicID,
                    TopicName = group.Key.TopicName,
                    AverageCorrectRate = Math.Round((double)avgCorrect, 2),
                    AverageStudyTime = Math.Round((double)avgTime, 2),
                    AverageTrendScore = Math.Round((double)avgTrend, 2),
                    Insight = insight
                });
            }

            return result;
        }


        public async Task<List<LearningActivity>> GetLearningActivitiesAsync(int userId, int month, int year)
        {
            var query = _context.LearningActivities
                .Where(a => a.UserID == userId);

            if (month > 0 && year > 0)
            {
                query = query.Where(a => a.StartTime.Month == month && a.StartTime.Year == year);
            }

            return await query
                .OrderBy(a => a.StartTime)
                .ToListAsync();
        }


        public async Task<StudentInfoDto> GetUserDetailsByIdAsync(int userId)
        {
            return await _context.Users
                .Where(u => u.UserID == userId)
                .Select(u => new StudentInfoDto
                {
                    StudentId = u.UserID,
                    Name = u.FullName,
                    Email = u.Email
                })
                .FirstOrDefaultAsync();
        }




        public async Task<List<EnhancedLearningReportDto>> GenerateEnhancedLearningReportAsync(int userId, int month, int year)
        {
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

                // Lấy các attempt
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

                var avgScore = attempts.Average(a => a.Score);
                var highScore = attempts.Max(a => a.Score);

                var goal = await _context.Goals
                    .Where(g => g.UserID == userId &&
                                g.TopicID == topicId &&
                                g.TargetDate.Value.Month == month &&
                                g.TargetDate.Value.Year == year)
                    .Select(g => new GoalDto
                    {
                        TargetScore = (double)(g.TargetScore ?? 0),
                        Description = g.GoalDescription
                    })
                    .FirstOrDefaultAsync();

                var report = new EnhancedLearningReportDto
                {
                    UserId = userId,
                    UserName = userName,
                    TopicId = topicId,
                    TopicName = topicName,
                    Month = month,
                    Year = year,
                    Goal = goal ?? new GoalDto
                    {
                        TargetScore = 0,
                        Description = "No goal set"
                    },
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
