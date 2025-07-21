using Synapse_API.Data;
using Microsoft.EntityFrameworkCore;
using Synapse_API.Models.Enums;

namespace Synapse_API.Repositories
{
    public class EventRepository
    {
        private readonly SynapseDbContext _context;

        public EventRepository(SynapseDbContext context)
        {
            _context = context;
        }

        public async Task<Models.Entities.Event> GetEventById(int id)
        {
            var e = await _context.Events.FindAsync(id);
            if (e == null)
            {
                return null;
            }
            return e;
        }

        public async Task<IEnumerable<Models.Entities.Event>> GetEventsByStudentId(int studentId)
        {
            var events = await _context.Events
                .Include(e => e.User)
                .Where(e => e.UserID == studentId)
                .ToListAsync();
            return events;
        }
        public async Task<IEnumerable<Models.Entities.Event>> GetParentEventsByStudentId(int studentId)
        {
            var events = await _context.Events
                .Include(e => e.User)
                .Where(e => e.UserID == studentId)
                .Where(e => e.EventType != EventType.StudySession)
                .ToListAsync();
            return events;
        }
        public async Task<Models.Entities.Event> GetEventAndItsSubEventById(int eventId)
        {
            var events = await _context.Events.Where(e => e.EventID == eventId).Include(e => e.ChildEvents).FirstOrDefaultAsync();
            return events;
        }
        public async Task<IEnumerable<Models.Entities.Event>> GetAllEvents()
        {
            var events = await _context.Events.ToListAsync();
            return events;
        }

        public async Task<Models.Entities.Event> CreateEvent(Models.Entities.Event e)
        {
            _context.Events.Add(e);
            await _context.SaveChangesAsync();
            return e;
        }

        public async Task<Models.Entities.Event> UpdateEvent(Models.Entities.Event e)
        {
            _context.Events.Update(e);
            await _context.SaveChangesAsync();
            return e;
        }

        public async Task<Models.Entities.Event> DeleteEvent(int id)
        {
            var e = await _context.Events.FindAsync(id);
            if (e == null)
            {
                return null;
            }
            var listSub = await DeleteChildEventsByParentEventId(e.EventID);
            _context.Events.Remove(e);
            await _context.SaveChangesAsync();
            return e;
        }
        public async Task<bool> DeleteChildEventsByParentEventId(int parentEventId)
        {
            var listSub = await _context.Events.Where(e => e.ParentEventID == parentEventId).ToListAsync();
            if (listSub == null) return true;
            _context.Events.RemoveRange(listSub);
            return true;
        }

        // Lấy events theo khoảng thời gian và user
        public async Task<IEnumerable<Models.Entities.Event>> GetEventsByUserAndDateRange(int userId, DateTime startDate, DateTime endDate)
        {
            return await _context.Events
                .Where(e => e.UserID == userId && 
                           e.StartTime >= startDate && 
                           e.StartTime <= endDate)
                .OrderBy(e => e.StartTime)
                .ToListAsync();
        }

        // Thêm method để lấy study sessions của một exam event
        public async Task<IEnumerable<Models.Entities.Event>> GetStudySessionsByParentEvent(int parentEventId)
        {
            return await _context.Events
                .Where(e => e.ParentEventID == parentEventId && e.EventType == EventType.StudySession)
                .OrderBy(e => e.StartTime)
                .ToListAsync();
        }

        // Thêm method để tạo nhiều events cùng lúc
        public async Task<IEnumerable<Models.Entities.Event>> CreateMultipleEvents(List<Models.Entities.Event> events)
        {
            _context.Events.AddRange(events);
            await _context.SaveChangesAsync();
            return events;
        }

        // Thêm method để xóa các study sessions cũ của một exam
        public async Task DeleteStudySessionsByParentEvent(int parentEventId)
        {
            var studySessions = await _context.Events
                .Where(e => e.ParentEventID == parentEventId && e.EventType == EventType.StudySession)
                .ToListAsync();
            
            _context.Events.RemoveRange(studySessions);
            await _context.SaveChangesAsync();
        }
    }
}
