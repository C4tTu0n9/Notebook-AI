
using Synapse_API.Models.Dto.ReminderDTOs;
using Synapse_API.Models.Entities;
using Synapse_API.Repositories;
using Synapse_API.Repositories.Event;
using Synapse_API.Services.AmazonServices;
using Microsoft.Extensions.Options;
using Synapse_API.Configuration_Services;

namespace Synapse_API.Services.EventServices
{
    public class EventReminderService
    {
        private readonly EmailService _emailService;
        private readonly EventRepository _eventRepository;
        private readonly ReminderRepository _reminderRepository;
        private readonly IOptions<ApplicationSettings> _appSettings;
        
        public EventReminderService(
            EmailService emailService,
            EventRepository eventRepository,
            ReminderRepository reminderRepository,
            IOptions<ApplicationSettings> appSettings)
        {
            _emailService = emailService;
            _eventRepository = eventRepository;
            _reminderRepository = reminderRepository;
            _appSettings = appSettings;
        }

        /// <summary>
        /// Kiểm tra và gửi email nhắc nhở cho các sự kiện sắp đến
        /// </summary>
        public async Task ProcessEventRemindersAsync()
        {
            try
            {
                var currentTime = DateTime.Now; //gio vn +7
                
                // Lấy tất cả reminder chưa gửi và đã đến thời gian gửi
                var dueReminders = await _reminderRepository.GetDueRemindersAsync(currentTime);
                foreach (var reminder in dueReminders)
                {
                    await SendReminderEmailAsync(reminder);
                }
                await _reminderRepository.SaveChangesAsync();
            }
            catch (Exception ex)
            {
            }
        }

        /// <summary>
        /// Gửi email nhắc nhở cho một reminder cụ thể
        /// </summary>
        private async Task SendReminderEmailAsync(EventReminder reminder)
        {
            try
            {
                var user = reminder.Event.User;
                var eventItem = reminder.Event;

                // Tính toán thời gian nhắc nhở
                var timeDifference = eventItem.StartTime - reminder.ReminderTime;
                var reminderTimeText = GetReminderTimeText(timeDifference);

                // Gửi email
                await _emailService.SendEventReminderEmailAsync(user, eventItem, reminderTimeText);

                // Đánh dấu đã gửi
                reminder.IsSent = true;
            }
            catch (Exception ex)
            {
                // Không throw để tiếp tục xử lý các reminder khác
            }
        }

        /// <summary>
        /// Tạo reminder tự động khi tạo sự kiện mới
        /// </summary>
        public async Task CreateDefaultRemindersAsync(int eventId, List<int> reminderMinutesBefore = null)
        {
            reminderMinutesBefore ??= _appSettings.Value.Reminder.DefaultValuesMinutesBefore.ToList();

            var eventItem = await _eventRepository.GetEventById(eventId);
            if (eventItem == null) return;

            foreach (var minutesBefore in reminderMinutesBefore)
            {
                var reminderTime = eventItem.StartTime.AddMinutes(-minutesBefore);
                
                // Chỉ tạo reminder nếu thời gian nhắc nhở là trong tương lai
                if (reminderTime > DateTime.Now)
                {
                    var reminder = new EventReminder
                    {
                        EventID = eventId,
                        ReminderTime = reminderTime,
                        IsSent = false
                    };
                    await _reminderRepository.CreateReminder(reminder);
                }
            }
        }

        /// <summary>
        /// Tạo reminder tùy chỉnh
        /// </summary>
        public async Task CreateCustomReminderAsync(int eventId, DateTime reminderTime)
        {
            var eventItem = await _eventRepository.GetEventById(eventId);
            if (eventItem == null) 
                throw new ArgumentException("Event does not exist");

            if (reminderTime <= DateTime.Now)
                throw new ArgumentException("Reminder time must be in the future");

            if (reminderTime >= eventItem.StartTime)
                throw new ArgumentException("Reminder time must be before the event start time");

            var reminder = new EventReminder
            {
                EventID = eventId,
                ReminderTime = reminderTime,
                IsSent = false
            };

            await _reminderRepository.CreateReminder(reminder);
        }


        /// <summary>
        /// Chuyển đổi thời gian thành text hiển thị
        /// </summary>
        private string GetReminderTimeText(TimeSpan timeDifference)
        {
            if (timeDifference.TotalMinutes < 60)
                return $"{(int)timeDifference.TotalMinutes} minutes";
            else if (timeDifference.TotalHours < 24)
                return $"{(int)timeDifference.TotalHours} hours";
            else
                return $"{(int)timeDifference.TotalDays} days";
        }


        // Event Reminder Management Methods
        public async Task<bool> CreateReminderAsync(int eventId, int? minutesBefore)
        {
            var eventItem = await _eventRepository.GetEventById(eventId);
            if (eventItem == null) return false;

            var reminderTime = eventItem.StartTime.AddMinutes((double)-minutesBefore);
            await CreateCustomReminderAsync(eventId, reminderTime);
            return true;
        }

        public async Task<bool> CreateReminderAsync(int eventId, DateTime reminderTime)
        {
            await CreateCustomReminderAsync(eventId, reminderTime);
            return true;
        }

        public async Task<IEnumerable<ReminderDto>> GetRemindersByEventIdAsync(int eventId)
        {
            var reminders = await _reminderRepository.GetRemindersByEventId(eventId);
            return reminders.Select(r => new ReminderDto
            {
                ReminderID = r.ReminderID,
                ReminderTime = r.ReminderTime,
                IsSent = r.IsSent
            });
        }

        public async Task<IEnumerable<ReminderDto>> GetRemindersByUserIdAsync(int userId, bool includeSent)
        {
            var reminders = await _reminderRepository.GetRemindersByUserId(userId, includeSent);
            return reminders.Select(r => new ReminderDto
            {
                ReminderID = r.ReminderID,
                ReminderTime = r.ReminderTime,
                IsSent = r.IsSent
            });
        }

        public async Task<bool> DeleteReminderAsync(int reminderId)
        {
            return await _reminderRepository.DeleteReminder(reminderId);
        }

        public async Task<bool> UpdateRemindersAsync(int eventId, List<int> reminderMinutesBefore)
        {
            // Xóa các reminder cũ chưa gửi
            await _reminderRepository.DeletePendingRemindersAsync(eventId);

            // Tạo reminder mới
            await CreateDefaultRemindersAsync(eventId, reminderMinutesBefore);
            return true;
        }
    }
} 