using Synapse_API.Models.Entities;
using Synapse_API.Repositories;

namespace Synapse_API.Services
{
    public class PerformanceMetricService
    {
        private readonly PerformanceMetricRepository _performanceMetricRepository;

        public PerformanceMetricService(PerformanceMetricRepository performanceMetricRepository)
        {
            _performanceMetricRepository = performanceMetricRepository;
        }

        /// <summary>
        /// Cập nhật PerformanceMetric sau khi user hoàn thành quiz
        /// </summary>
        public async Task UpdatePerformanceMetricAsync(int userId, int topicId)
        {
            // 1. Lấy hoặc tạo mới PerformanceMetric
            var existingMetric = await _performanceMetricRepository.GetByUserAndTopicAsync(userId, topicId);

            // 2. Lấy tất cả quiz attempts của user trong topic này
            var allAttempts = await _performanceMetricRepository.GetAllQuizAttemptsByUserAndTopicAsync(userId, topicId);

            if (allAttempts.Count == 0)
                return; // Không có attempts nào, không cần cập nhật

            // 3. Tính CorrectRate: số câu đúng / tổng số câu trong tất cả quiz của topic
            var correctRate = CalculateCorrectRate(allAttempts, topicId);

            // 4. Tính TrendScore: điểm số xuất hiện nhiều lần nhất
            var trendScore = CalculateTrendScore(allAttempts);

            // 5. Cập nhật hoặc tạo mới metric
            if (existingMetric != null)
            {
                existingMetric.CorrectRate = correctRate;
                existingMetric.TrendScore = trendScore;
                existingMetric.LastUpdated = DateTime.Now;
                await _performanceMetricRepository.UpdateAsync(existingMetric);
            }
            else
            {
                var newMetric = new PerformanceMetric
                {
                    UserID = userId,
                    TopicID = topicId,
                    CorrectRate = correctRate,
                    TrendScore = trendScore,
                    LastUpdated = DateTime.Now
                };
                await _performanceMetricRepository.CreateAsync(newMetric);
            }
        }

        /// <summary>
        /// Tính CorrectRate: số câu trả lời đúng / tổng số câu trong tất cả quiz của topic
        /// </summary>
        private decimal CalculateCorrectRate(List<UserQuizAttempt> attempts, int topicId)
        {
            var totalCorrectAnswers = 0;
            var totalQuestions = 0;

            foreach (var attempt in attempts)
            {
                // Đếm số câu trả lời đúng trong mỗi attempt
                var correctAnswersInAttempt = attempt.UserAnswers.Count(ua => ua.IsCorrect == true);
                totalCorrectAnswers += correctAnswersInAttempt;

                // Đếm tổng số câu hỏi trong quiz này
                var questionsInQuiz = attempt.UserAnswers.Count;
                totalQuestions += questionsInQuiz;
            }

            if (totalQuestions == 0)
                return 0;

            // Trả về tỷ lệ phần trăm
            return Math.Round((decimal)totalCorrectAnswers / totalQuestions * 100, 2);
        }

        /// <summary>
        /// Tính TrendScore: điểm số xuất hiện nhiều lần nhất trong tất cả quiz của topic
        /// </summary>
        private decimal CalculateTrendScore(List<UserQuizAttempt> attempts)
        {
            if (attempts.Count == 0)
                return 0;

            // Nhóm theo điểm số và đếm số lần xuất hiện
            var scoreFrequency = attempts
                .Where(a => a.Score.HasValue)
                .GroupBy(a => Math.Round(a.Score.Value, 1)) // Làm tròn điểm số đến 1 chữ số thập phân
                .ToDictionary(g => g.Key, g => g.Count());

            if (scoreFrequency.Count == 0)
                return 0;

            // Tìm điểm số có tần suất cao nhất
            var mostFrequentScore = scoreFrequency
                .OrderByDescending(kvp => kvp.Value) // Sắp xếp theo tần suất giảm dần
                .ThenByDescending(kvp => kvp.Key)    // Nếu tần suất bằng nhau, ưu tiên điểm cao hơn
                .First()
                .Key;

            return mostFrequentScore;
        }

        /// <summary>
        /// Lấy PerformanceMetric theo UserID và TopicID
        /// </summary>
        public async Task<PerformanceMetric?> GetPerformanceMetricAsync(int userId, int topicId)
        {
            return await _performanceMetricRepository.GetByUserAndTopicAsync(userId, topicId);
        }
    }
} 