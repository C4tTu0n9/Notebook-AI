using Microsoft.EntityFrameworkCore;
using Synapse_API.Data;
using Synapse_API.Models.Entities;

namespace Synapse_API.Repositories
{
    public class LearningActivityRepository
    {
        private readonly SynapseDbContext _context;

        public LearningActivityRepository(SynapseDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(LearningActivity activity)
        {
            _context.LearningActivities.Add(activity);
            await _context.SaveChangesAsync();
        }

        public async Task<List<LearningActivity>> GetAllLearningActivity()
        {
            return  await _context.LearningActivities.ToListAsync();
        }


    }
}
