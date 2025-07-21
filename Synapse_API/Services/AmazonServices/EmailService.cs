using Amazon.SimpleEmail;
using Amazon.SimpleEmail.Model;
using Microsoft.Extensions.Options;
using Synapse_API.Models.Config;
using Synapse_API.Models.Entities;
using Synapse_API.Utils;

namespace Synapse_API.Services.AmazonServices
{
    public class EmailService
    {
        private readonly IAmazonSimpleEmailService _sesClient;
        private readonly AwsOptions _awsOptions;

        public EmailService(IAmazonSimpleEmailService sesClient, IOptions<AwsOptions> options)
        {
            _sesClient = sesClient;
            _awsOptions = options.Value;
        }

        public async Task SendEmailAsync(string to, string title, string content)
        {
            var template = await EmailHelper.LoadEmailTemplate("WelcomeTemplate.html");
            var htmlBody = string.Format(template, to, content);
            var sendRequest = new SendEmailRequest
            {
                Source = _awsOptions.SesSender,
                Destination = new Destination { ToAddresses = new List<string> { to } },
                Message = new Message
                {
                    Subject = new Content(title),
                    Body = new Body { Html = new Content(htmlBody) }
                }
            };

            await _sesClient.SendEmailAsync(sendRequest);
        }

        public async Task SendEventReminderEmailAsync(User user, Event eventItem, string reminderTimeText)
        {
            var template = await EmailHelper.LoadEmailTemplate("EventReminderTemplate.html");
            
            var description = !string.IsNullOrEmpty(eventItem.Description) 
                ? $"<div class=\"event-description\">{eventItem.Description}</div>" 
                : "";

            var htmlBody = string.Format(template,
                user.FullName,                                           // {0} - Tên người dùng
                reminderTimeText,                                        // {1} - Thời gian nhắc nhở
                eventItem.Title,                                         // {2} - Tiêu đề sự kiện
                eventItem.StartTime.ToString("dd/MM/yyyy"),             // {3} - Ngày sự kiện
                eventItem.StartTime.ToString("HH:mm"),                  // {4} - Giờ bắt đầu
                eventItem.EndTime.ToString("HH:mm"),                    // {5} - Giờ kết thúc
                GetEventTypeDisplayName(eventItem.EventType),           // {6} - Loại sự kiện
                description                                              // {7} - Mô tả
            );

            var sendRequest = new SendEmailRequest
            {
                Source = _awsOptions.SesSender,
                Destination = new Destination { ToAddresses = new List<string> { user.Email } },
                Message = new Message
                {
                    Subject = new Content($"🔔 Reminder: {eventItem.Title}"),
                    Body = new Body { Html = new Content(htmlBody) }
                }
            };

            await _sesClient.SendEmailAsync(sendRequest);
        }

        private string GetEventTypeDisplayName(Models.Enums.EventType eventType)
        {
            return eventType switch
            {
                Models.Enums.EventType.Class => "Class",
                Models.Enums.EventType.Assignment => "Assignment",
                Models.Enums.EventType.Exam => "Exam",
                Models.Enums.EventType.StudySession => "Study Session",
                Models.Enums.EventType.Other => "Other",
                _ => eventType.ToString()
            };
        }
    }
}
