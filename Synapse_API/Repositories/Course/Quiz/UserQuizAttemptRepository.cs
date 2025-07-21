using Microsoft.EntityFrameworkCore;
using Synapse_API.Data;
using Synapse_API.Models.Entities;
using Synapse_API.Services.DatabaseServices;

namespace Synapse_API.Repositories.Course.Quiz
{
    public class UserQuizAttemptRepository
    {
        private readonly SynapseDbContext _context;
        private readonly RedisService _redisService;

        public UserQuizAttemptRepository(SynapseDbContext context, RedisService redisService)
        {
            _context = context;
            _redisService = redisService;
        }

        public async Task<UserQuizAttempt> CreateAttemptAsync(UserQuizAttempt attempt)
        {
            await _context.UserQuizAttempts.AddAsync(attempt);
            await _context.SaveChangesAsync();
            return attempt;
        }

        public async Task<UserQuizAttempt?> GetAttemptByIdAsync(int attemptId)
        {
            return await _context.UserQuizAttempts
                .Include(a => a.Quiz)
                .Include(a => a.User)
                .Include(a => a.UserAnswers)
                    .ThenInclude(ua => ua.Question)
                        .ThenInclude(q => q.Options)
                .FirstOrDefaultAsync(a => a.AttemptID == attemptId);
        }

        public async Task<List<UserQuizAttempt>> GetAttemptsByUserIdAsync(int userId)
        {
            return await _context.UserQuizAttempts
                .Include(a => a.Quiz)
                .Where(a => a.UserID == userId)
                .OrderByDescending(a => a.AttemptDate)
                .ToListAsync();
        }

        public async Task<List<UserQuizAttempt>> GetAttemptsByQuizIdAsync(int quizId)
        {
            return await _context.UserQuizAttempts
                .Include(a => a.User)
                .Where(a => a.QuizID == quizId)
                .OrderByDescending(a => a.AttemptDate)
                .ToListAsync();
        }

        public async Task<UserQuizAttempt?> GetLatestAttemptByUserAndQuizAsync(int userId, int quizId)
        {
            return await _context.UserQuizAttempts
                .Include(a => a.UserAnswers)
                    .ThenInclude(ua => ua.Question)
                        .ThenInclude(q => q.Options)
                .Where(a => a.UserID == userId && a.QuizID == quizId)
                .OrderByDescending(a => a.AttemptDate)
                .FirstOrDefaultAsync();
        }

        public async Task UpdateAttemptAsync(UserQuizAttempt attempt)
        {
            _context.UserQuizAttempts.Update(attempt);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> HasUserAttemptedQuizAsync(int userId, int quizId)
        {
            return await _context.UserQuizAttempts
                .AnyAsync(a => a.UserID == userId && a.QuizID == quizId);
        }

        public async Task<int> GetAttemptCountByUserAndQuizAsync(int userId, int quizId)
        {
            return await _context.UserQuizAttempts
                .CountAsync(a => a.UserID == userId && a.QuizID == quizId);
        }
    }
} 