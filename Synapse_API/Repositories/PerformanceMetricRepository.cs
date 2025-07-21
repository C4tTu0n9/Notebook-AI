using Microsoft.EntityFrameworkCore;
using Synapse_API.Data;
using Synapse_API.Models.Entities;
using Synapse_API.Services.DatabaseServices;

namespace Synapse_API.Repositories
{
    public class PerformanceMetricRepository
    {
        private readonly SynapseDbContext _context;
        private readonly RedisService _redisService;

        public PerformanceMetricRepository(SynapseDbContext context, RedisService redisService)
        {
            _context = context;
            _redisService = redisService;
        }

        /// <summary>
        /// Tìm PerformanceMetric theo UserID và TopicID
        /// </summary>
        public async Task<PerformanceMetric?> GetByUserAndTopicAsync(int userId, int topicId)
        {
            return await _context.PerformanceMetrics
                .FirstOrDefaultAsync(pm => pm.UserID == userId && pm.TopicID == topicId);
        }

        /// <summary>
        /// Tạo mới PerformanceMetric
        /// </summary>
        public async Task<PerformanceMetric> CreateAsync(PerformanceMetric performanceMetric)
        {
            await _context.PerformanceMetrics.AddAsync(performanceMetric);
            await _context.SaveChangesAsync();
            return performanceMetric;
        }

        /// <summary>
        /// Cập nhật PerformanceMetric
        /// </summary>
        public async Task<PerformanceMetric> UpdateAsync(PerformanceMetric performanceMetric)
        {
            _context.PerformanceMetrics.Update(performanceMetric);
            await _context.SaveChangesAsync();
            return performanceMetric;
        }

        /// <summary>
        /// Lấy tất cả quiz attempts của user trong một topic cụ thể
        /// </summary>
        public async Task<List<UserQuizAttempt>> GetAllQuizAttemptsByUserAndTopicAsync(int userId, int topicId)
        {
            return await _context.UserQuizAttempts
                .Include(uqa => uqa.Quiz)
                .Include(uqa => uqa.UserAnswers)
                    .ThenInclude(ua => ua.Question)
                        .ThenInclude(q => q.Options)
                .Where(uqa => uqa.UserID == userId && uqa.Quiz.TopicID == topicId)
                .OrderByDescending(uqa => uqa.AttemptDate)
                .ToListAsync();
        }

        /// <summary>
        /// Lấy tất cả questions từ các quiz trong topic
        /// </summary>
        public async Task<List<Question>> GetAllQuestionsInTopicAsync(int topicId)
        {
            return await _context.Questions
                .Include(q => q.Quiz)
                .Where(q => q.Quiz.TopicID == topicId)
                .ToListAsync();
        }
    }
} 