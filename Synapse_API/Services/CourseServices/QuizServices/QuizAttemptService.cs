using AutoMapper;
using Synapse_API.Models.Dto.QuizDTOs;
using Synapse_API.Models.Entities;
using Synapse_API.Repositories.Course.Quiz;
using Synapse_API.Services.DatabaseServices;
using Microsoft.Extensions.Options;
using Synapse_API.Configuration_Services;
namespace Synapse_API.Services.CourseServices.QuizServices
{
    public class QuizAttemptService
    {
        private readonly UserQuizAttemptRepository _attemptRepository;
        private readonly UserAnswerRepository _answerRepository;
        private readonly QuizRepository _quizRepository;
        private readonly IMapper _mapper;
        private readonly RedisService _redisService;
        private readonly PerformanceMetricService _performanceMetricService;
        private readonly IOptions<ApplicationSettings> _appSettings;

        public QuizAttemptService(
            UserQuizAttemptRepository attemptRepository,
            UserAnswerRepository answerRepository,
            QuizRepository quizRepository,
            IMapper mapper,
            RedisService redisService,
            PerformanceMetricService performanceMetricService,
            IOptions<ApplicationSettings> appSettings)
        {
            _attemptRepository = attemptRepository;
            _answerRepository = answerRepository;
            _quizRepository = quizRepository;
            _mapper = mapper;
            _redisService = redisService;
            _performanceMetricService = performanceMetricService;
            _appSettings = appSettings;
        }

        /// <summary>
        /// Submit quiz và chấm điểm tự động
        /// </summary>
        public async Task<QuizAttemptResponseDto> SubmitQuizAsync(int userId, SubmitQuizRequest request)
        {
            // 1. Lấy thông tin quiz với questions và options
            var quiz = await _quizRepository.GetQuizWithQuestionsAsync(request.QuizID);
            if (quiz == null)
                throw new ArgumentException("Quiz không tồn tại");

            // 2. Tạo quiz attempt mới
            var attempt = new UserQuizAttempt
            {
                UserID = userId,
                QuizID = request.QuizID,
                AttemptDate = DateTime.Now,
                Score = 0, // Sẽ được tính sau
                Feedback = ""
            };

            var createdAttempt = await _attemptRepository.CreateAttemptAsync(attempt);

            // 3. Xử lý từng câu trả lời và chấm điểm
            var userAnswers = new List<UserAnswer>();
            var questionResults = new List<QuestionResultDto>();
            int correctCount = 0;

            foreach (var userAnswer in request.UserAnswers)
            {
                var question = quiz.Questions.FirstOrDefault(q => q.QuestionID == userAnswer.QuestionID);
                if (question == null) continue;

                // Tìm đáp án đúng
                var correctOption = question.Options.FirstOrDefault(o => o.IsCorrect);
                var isCorrect = correctOption?.OptionKey == userAnswer.SelectedOption;

                if (isCorrect) correctCount++;

                // Tạo UserAnswer entity
                var answerEntity = new UserAnswer
                {
                    AttemptID = createdAttempt.AttemptID,
                    QuestionID = userAnswer.QuestionID,
                    SelectedOption = userAnswer.SelectedOption,
                    IsCorrect = isCorrect
                };

                userAnswers.Add(answerEntity);

                // Tạo QuestionResult cho response
                var questionResult = new QuestionResultDto
                {
                    QuestionID = question.QuestionID,
                    QuestionText = question.QuestionText,
                    SelectedOption = userAnswer.SelectedOption,
                    CorrectOption = correctOption?.OptionKey ?? "",
                    IsCorrect = isCorrect,
                    Explanation = question.Explanation
                };

                questionResults.Add(questionResult);
            }

            // 4. Lưu tất cả câu trả lời
            await _answerRepository.CreateMultipleAnswersAsync(userAnswers);

            // 5. Tính điểm số (thang điểm 10)
            decimal score = CalculateScore(correctCount, quiz.Questions.Count);

            // 6. Tạo feedback
            string feedback = GenerateFeedback(correctCount, quiz.Questions.Count, score);

            // 7. Cập nhật attempt với điểm số và feedback
            createdAttempt.Score = score;
            createdAttempt.Feedback = feedback;
            await _attemptRepository.UpdateAttemptAsync(createdAttempt);

            // 8. Cập nhật PerformanceMetric cho topic này
            await _performanceMetricService.UpdatePerformanceMetricAsync(userId, quiz.TopicID);

            // 9. Trả về kết quả
            return new QuizAttemptResponseDto
            {
                AttemptID = createdAttempt.AttemptID,
                UserID = userId,
                QuizID = request.QuizID,
                Score = score,
                AttemptDate = createdAttempt.AttemptDate,
                Feedback = feedback,
                QuestionResults = questionResults
            };
        }

        /// <summary>
        /// Lấy kết quả quiz attempt
        /// </summary>
        public async Task<QuizAttemptResponseDto?> GetQuizAttemptResultAsync(int attemptId)
        {
            var attempt = await _attemptRepository.GetAttemptByIdAsync(attemptId);
            if (attempt == null) return null;

            var questionResults = attempt.UserAnswers.Select(ua => new QuestionResultDto
            {
                QuestionID = ua.QuestionID,
                QuestionText = ua.Question.QuestionText,
                SelectedOption = ua.SelectedOption,
                CorrectOption = ua.Question.Options.FirstOrDefault(o => o.IsCorrect)?.OptionKey ?? "",
                IsCorrect = ua.IsCorrect ?? false,
                Explanation = ua.Question.Explanation
            }).ToList();

            return new QuizAttemptResponseDto
            {
                AttemptID = attempt.AttemptID,
                UserID = attempt.UserID,
                QuizID = attempt.QuizID,
                Score = attempt.Score ?? 0,
                AttemptDate = attempt.AttemptDate,
                Feedback = attempt.Feedback,
                QuestionResults = questionResults
            };
        }

        /// <summary>
        /// Lấy danh sách attempts của user
        /// </summary>
        public async Task<List<QuizAttemptResponseDto>> GetUserAttemptHistoryAsync(int userId)
        {
            var attempts = await _attemptRepository.GetAttemptsByUserIdAsync(userId);
            return attempts.Select(a => new QuizAttemptResponseDto
            {
                AttemptID = a.AttemptID,
                UserID = a.UserID,
                QuizID = a.QuizID,
                Score = a.Score ?? 0,
                AttemptDate = a.AttemptDate,
                Feedback = a.Feedback,
                QuestionResults = new List<QuestionResultDto>() // Không load chi tiết để tối ưu performance
            }).ToList();
        }

        /// <summary>
        /// Kiểm tra user đã làm quiz chưa
        /// </summary>
        public async Task<bool> HasUserAttemptedQuizAsync(int userId, int quizId)
        {
            return await _attemptRepository.HasUserAttemptedQuizAsync(userId, quizId);
        }

        /// <summary>
        /// Tạo feedback dựa trên kết quả
        /// </summary>
        private string GenerateFeedback(int correctCount, int totalQuestions, decimal score, string lang = "vi")
        {
            var thresholds = _appSettings.Value.Quiz.FeedbackThresholds;
            var messages = _appSettings.Value.Quiz.FeedbackMessages;

            string label = thresholds
                .OrderByDescending(t => t.Value)
                .FirstOrDefault(t => score >= t.Value).Key ?? "BelowAverage";

            var langMessages = messages[lang];
            var template = langMessages[label];

            return template
                .Replace("{correct}", correctCount.ToString())
                .Replace("{total}", totalQuestions.ToString())
                .Replace("{score}", score.ToString($"F{_appSettings.Value.Quiz.ScoreDecimalPlaces}"));
        }


        public async Task<List<QuizAttemptResponseDto>> GetQuizResultByQuizID(int quizID)
        {
            var attempts = await _attemptRepository.GetAttemptsByQuizIdAsync(quizID);
            return attempts.Select(a => new QuizAttemptResponseDto
            {
                AttemptID = a.AttemptID,
                UserID = a.UserID,
                QuizID = a.QuizID,
                Score = a.Score ?? 0,
                AttemptDate = a.AttemptDate,
                Feedback = a.Feedback,
                QuestionResults = new List<QuestionResultDto>()
            }).ToList();
        }

        private decimal CalculateScore(int correctCount, int totalQuestions)
        {
            if (totalQuestions == 0) return 0;

            var score = (decimal)correctCount / totalQuestions * _appSettings.Value.Quiz.MaxScore;
            return Math.Round(score, _appSettings.Value.Quiz.ScoreDecimalPlaces);
        }
    }
}