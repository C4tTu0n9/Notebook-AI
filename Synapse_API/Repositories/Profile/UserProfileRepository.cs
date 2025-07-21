using Microsoft.EntityFrameworkCore;
using Synapse_API.Data;
using Synapse_API.Models.Entities;
using Synapse_API.Services.DatabaseServices;

namespace Synapse_API.Repositories.Profile
{
    public class UserProfileRepository
    {
        private readonly SynapseDbContext _context;
        private readonly RedisService _redisService;
        public UserProfileRepository(SynapseDbContext context, RedisService redisService)
        {
            _context = context;
            _redisService = redisService;
        }
        public async Task<UserProfile?> GetUserProfileByUserId(int userId)
        {
            return await _context.UserProfiles
                .FirstOrDefaultAsync(up => up.UserID == userId);
        }
    }
}
