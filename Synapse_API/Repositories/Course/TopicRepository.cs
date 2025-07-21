using Microsoft.EntityFrameworkCore;
using Synapse_API.Data;

namespace Synapse_API.Repositories.Course
{
    public class TopicRepository
    {
        private readonly SynapseDbContext _context;

        public TopicRepository(SynapseDbContext context)
        {
            _context = context;
        }
        public async Task<Models.Entities.Topic?> GetTopicById(int? topicId)
        {
            return await _context.Topics.FirstOrDefaultAsync(t => t.TopicID == topicId);
        }
        public async Task<List<Models.Entities.Topic>> GetAllTopics()
        {
            return await _context.Topics.ToListAsync();
        }
        public async Task<Models.Entities.Topic> AddTopic(Models.Entities.Topic topic)
        {
            _context.Topics.Add(topic);
            await _context.SaveChangesAsync();
            return topic;
        }
        public async Task<Models.Entities.Topic> UpdateTopic(Models.Entities.Topic topic)
        {
            _context.Topics.Update(topic);
            await _context.SaveChangesAsync();
            return topic;
        }
        public async Task<Models.Entities.Topic> DeleteTopic(Models.Entities.Topic topic)
        {
            _context.Topics.Remove(topic);
            await _context.SaveChangesAsync();
            return topic;
        }
        public async Task<List<Models.Entities.Topic>> GetTopicByCourseId(int courseId)
        {
            return await _context.Topics.Where(t => t.CourseID == courseId).ToListAsync();
        }
    }
}
