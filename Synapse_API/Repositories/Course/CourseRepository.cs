using Microsoft.EntityFrameworkCore;
using Synapse_API.Data;
using Synapse_API.Services.DatabaseServices;

namespace Synapse_API.Repositories.Course
{
    public class CourseRepository
    {
        private readonly SynapseDbContext _context;
        private readonly RedisService _redisService;
        public CourseRepository(SynapseDbContext context, RedisService redisService)
        {
            _context = context;
            _redisService = redisService;
        }

        public async Task<Models.Entities.Course?> GetCourseById(int courseId)
        {
            return await _context.Courses
                .Include(c => c.Topics)
                .FirstOrDefaultAsync(c => c.CourseID == courseId);
        }
        // lấy toàn bộ course
        public async Task<List<Models.Entities.Course>> GetAllCourseAsync()
        {
            return await _context.Courses.ToListAsync();
        }
        // Create course mới
        public async Task<Models.Entities.Course> CreateCourseAsync(Models.Entities.Course course)
        {
            await _context.Courses.AddAsync(course);
            await _context.SaveChangesAsync();
            return course;
        } 
        // Update course
        public async Task<Models.Entities.Course> UpdateCourseAsync(Models.Entities.Course course)
        {
             _context.Courses.Update(course);
            await _context.SaveChangesAsync();
            return course;
        }
        // Delete course
        public async Task<Models.Entities.Course> DeleteCourseAsync(Models.Entities.Course course)
        {
            _context.Courses.Remove(course);
            await _context.SaveChangesAsync();
            return course;
        }
        // Lấy lấy danh sách evnet theo Course
        public async Task<List<Models.Entities.Event>>GetListParentEventByCourseId(int courseId)
        {
           return await _context.Events
                .Where(e => e.CourseID == courseId)
                .Where(e => e.ParentEventID == null)
                .OrderByDescending(e => e.StartTime)
                .ToListAsync();
        }

        public async Task<List<Models.Entities.Course>> GetCourseByUserId(int userId)
        {
            return await _context.Courses
                .Where(c => c.UserID == userId)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }
    }
}
