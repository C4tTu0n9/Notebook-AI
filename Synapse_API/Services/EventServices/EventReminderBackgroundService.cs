using Synapse_API.Services.EventServices;
using Microsoft.Extensions.Options;
using Synapse_API.Configuration_Services;

namespace Synapse_API.Services.EventServices
{
    public class EventReminderBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<EventReminderBackgroundService> _logger;
        private readonly TimeSpan _checkInterval;

        public EventReminderBackgroundService(
            IServiceProvider serviceProvider,
            ILogger<EventReminderBackgroundService> logger,
            IOptions<ApplicationSettings> appSettings)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _checkInterval = TimeSpan.FromMinutes(appSettings.Value.BackgroundJob.ReminderCheckIntervalMinutes);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessRemindersAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Lỗi Event Reminder Background Service");
                }

                // Chờ interval trước khi check tiếp
                await Task.Delay(_checkInterval, stoppingToken);
           }
        }

        private async Task ProcessRemindersAsync()
        {
            // Tạo scope mới để có fresh DbContext
            using var scope = _serviceProvider.CreateScope();
            var reminderService = scope.ServiceProvider.GetRequiredService<EventReminderService>();

            try
            {
                await reminderService.ProcessEventRemindersAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xử lý event reminders trong background service");
                throw;
            }
        }

        public override async Task StopAsync(CancellationToken stoppingToken)
        {
            await base.StopAsync(stoppingToken);
        }
    }
} 