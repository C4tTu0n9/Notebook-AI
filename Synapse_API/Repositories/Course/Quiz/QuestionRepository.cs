using Synapse_API.Data;
using Synapse_API.Services.DatabaseServices;

namespace Synapse_API.Repositories.Course.Quiz
{
    public class QuestionRepository
    {
        private readonly SynapseDbContext _context;
        private readonly RedisService _redisService;
        public QuestionRepository(SynapseDbContext context, RedisService redisService)
        {
            _context = context;
            _redisService = redisService;
        }
        public async Task<Models.Entities.Question> CreateQuestion(Models.Entities.Question question)
        {
            await _context.Questions.AddAsync(question);
            await _context.SaveChangesAsync();
            return question;
        }
    }
}
