using Synapse_API.Services.EventServices;

namespace Synapse_API.Services.EventServices
{
    public class EventReminderBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<EventReminderBackgroundService> _logger;
        private readonly TimeSpan _checkInterval;

        public EventReminderBackgroundService(
            IServiceProvider serviceProvider,
            ILogger<EventReminderBackgroundService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _checkInterval = TimeSpan.FromMinutes(1); // Kiểm tra mỗi 1 phút
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Event Reminder Background Service đã khởi động");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessRemindersAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Lỗi trong Event Reminder Background Service");
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
            _logger.LogInformation("Event Reminder Background Service đang dừng...");
            await base.StopAsync(stoppingToken);
        }
    }
} 