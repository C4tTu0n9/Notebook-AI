using Synapse_API.Data;
using Synapse_API.Services.DatabaseServices;

namespace Synapse_API.Repositories.Course.Quiz
{
    public class OptionRepository
    {
        private readonly SynapseDbContext _context;
        private readonly RedisService _redisService;
        public OptionRepository(SynapseDbContext context, RedisService redisService)
        {
            _context = context;
            _redisService = redisService;
        }
        public async Task<Models.Entities.Option> CreateOption(Models.Entities.Option option)
        {
            await _context.Options.AddAsync(option);
            await _context.SaveChangesAsync();
            return option;
        }
    }
}
