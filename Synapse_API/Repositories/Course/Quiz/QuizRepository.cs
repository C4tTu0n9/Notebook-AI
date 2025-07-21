using Microsoft.EntityFrameworkCore;
using Synapse_API.Data;
using Synapse_API.Services.DatabaseServices;

namespace Synapse_API.Repositories.Course.Quiz
{
    public class QuizRepository
    {
        private readonly SynapseDbContext _context;
        private readonly RedisService _redisService;
        public QuizRepository(SynapseDbContext context, RedisService redisService)
        {
            _context = context;
            _redisService = redisService;
        }

        public async Task<Models.Entities.Quiz> CreateQuiz(Models.Entities.Quiz quiz)
        {
            await _context.Quizzes.AddAsync(quiz);
            await _context.SaveChangesAsync();
            return quiz;
        }

        public async Task<List<Models.Entities.Quiz>> GetQuizByTopicId(int topicId)
        {
            var quiz = await _context.Quizzes.Where(q => q.TopicID == topicId).ToListAsync();
            return quiz;
        }

        public async Task<Models.Entities.Quiz?> GetQuizWithQuestionsAsync(int quizId)
        {
            return await _context.Quizzes
                .Include(q => q.Questions)
                    .ThenInclude(qu => qu.Options)
                .FirstOrDefaultAsync(q => q.QuizID == quizId);
        }

        public async Task DeleteQuiz(int quizID)
        {
            var quiz = await _context.Quizzes.FirstOrDefaultAsync(q => q.QuizID == quizID);
            if (quiz != null)
            {
                _context.Quizzes.Remove(quiz);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<Models.Entities.Quiz?> GetQuizById(int quizID)
        {
            return await _context.Quizzes.FirstOrDefaultAsync(q => q.QuizID == quizID);
        }
    }
}
