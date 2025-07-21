using Microsoft.EntityFrameworkCore;
using Synapse_API.Data;
using Synapse_API.Models.Entities;
using Synapse_API.Services.DatabaseServices;

namespace Synapse_API.Repositories.Course.Quiz
{
    public class UserAnswerRepository
    {
        private readonly SynapseDbContext _context;
        private readonly RedisService _redisService;

        public UserAnswerRepository(SynapseDbContext context, RedisService redisService)
        {
            _context = context;
            _redisService = redisService;
        }

        public async Task<UserAnswer> CreateAnswerAsync(UserAnswer answer)
        {
            await _context.UserAnswers.AddAsync(answer);
            await _context.SaveChangesAsync();
            return answer;
        }

        public async Task<List<UserAnswer>> CreateMultipleAnswersAsync(List<UserAnswer> answers)
        {
            await _context.UserAnswers.AddRangeAsync(answers);
            await _context.SaveChangesAsync();
            return answers;
        }

        public async Task<List<UserAnswer>> GetAnswersByAttemptIdAsync(int attemptId)
        {
            return await _context.UserAnswers
                .Include(ua => ua.Question)
                    .ThenInclude(q => q.Options)
                .Where(ua => ua.AttemptID == attemptId)
                .ToListAsync();
        }

        public async Task<UserAnswer?> GetAnswerByAttemptAndQuestionAsync(int attemptId, int questionId)
        {
            return await _context.UserAnswers
                .Include(ua => ua.Question)
                    .ThenInclude(q => q.Options)
                .FirstOrDefaultAsync(ua => ua.AttemptID == attemptId && ua.QuestionID == questionId);
        }

        public async Task UpdateAnswerAsync(UserAnswer answer)
        {
            _context.UserAnswers.Update(answer);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAnswerAsync(int answerId)
        {
            var answer = await _context.UserAnswers.FindAsync(answerId);
            if (answer != null)
            {
                _context.UserAnswers.Remove(answer);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<int> GetCorrectAnswersCountByAttemptAsync(int attemptId)
        {
            return await _context.UserAnswers
                .Where(ua => ua.AttemptID == attemptId && ua.IsCorrect == true)
                .CountAsync();
        }

        public async Task<int> GetTotalAnswersCountByAttemptAsync(int attemptId)
        {
            return await _context.UserAnswers
                .Where(ua => ua.AttemptID == attemptId)
                .CountAsync();
        }
    }
} 