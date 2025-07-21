namespace Synapse_API.Models.Dto.ReminderDTOs
{
    public class CreateReminderRequest
    {
        public int? MinutesBefore { get; set; }
        public DateTime? ReminderTime { get; set; }
    }
}
