using Microsoft.EntityFrameworkCore;
using Synapse_API.Data;
using Synapse_API.Models.Entities;

namespace Synapse_API.Repositories.Event
{
    public class ReminderRepository
    {
        private readonly SynapseDbContext _context;
        public ReminderRepository(SynapseDbContext context)
        {
            _context = context;
        }

        // EventReminder related methods
        public async Task<EventReminder> CreateReminder(EventReminder reminder)
        {
            _context.EventReminders.Add(reminder);
            await _context.SaveChangesAsync();
            return reminder;
        }

        public async Task<IEnumerable<EventReminder>> GetRemindersByEventId(int eventId)
        {
            return await _context.EventReminders
                .Where(r => r.EventID == eventId)
                .OrderBy(r => r.ReminderTime)
                .ToListAsync();
        }

        public async Task<IEnumerable<EventReminder>> GetRemindersByUserId(int userId, bool includeSent = false)
        {
            var query = _context.EventReminders
                .Include(r => r.Event)
                .Where(r => r.Event.UserID == userId);

            if (!includeSent)
            {
                query = query.Where(r => !r.IsSent);
            }

            return await query.OrderBy(r => r.ReminderTime).ToListAsync();
        }

        public async Task<EventReminder> UpdateReminder(EventReminder reminder)
        {
            _context.EventReminders.Update(reminder);
            await _context.SaveChangesAsync();
            return reminder;
        }

        public async Task<bool> DeleteReminder(int reminderId)
        {
            var reminder = await _context.EventReminders.FindAsync(reminderId);
            if (reminder == null) return false;

            _context.EventReminders.Remove(reminder);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteRemindersByEventId(int eventId)
        {
            var reminders = await _context.EventReminders
                .Where(r => r.EventID == eventId)
                .ToListAsync();

            _context.EventReminders.RemoveRange(reminders);
            await _context.SaveChangesAsync();
            return true;
        }
        /// <summary>
        /// Xóa các reminder chưa gửi của một sự kiện
        /// </summary>
        public async Task DeletePendingRemindersAsync(int eventId)
        {
            var pendingReminders = await _context.EventReminders
                .Where(r => r.EventID == eventId && !r.IsSent)
                .ToListAsync();

            _context.EventReminders.RemoveRange(pendingReminders);
            await _context.SaveChangesAsync();
        }
        /// <summary>
        /// Get all reminder của 1 user
        /// </summary>
        public async Task<List<EventReminder>> GetUserRemindersAsync(int userId, bool includeSent = false)
        {
            var query = _context.EventReminders
                .Include(r => r.Event)
                .Where(r => r.Event.UserID == userId);

            if (!includeSent)
            {
                query = query.Where(r => !r.IsSent);
            }

            return await query.OrderBy(r => r.ReminderTime).ToListAsync();
        }

        public async Task<IEnumerable<EventReminder>> GetDueRemindersAsync(DateTime currentTime)
        {
            return await _context.EventReminders
                    .Include(r => r.Event)
                        .ThenInclude(e => e.User)
                    .Where(r => !r.IsSent && r.ReminderTime <= currentTime)
                    .ToListAsync();
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
