namespace Synapse_API.Models.Dto.ReminderDTOs
{
    public class UpdateRemindersRequest
    {
        public List<int> ReminderMinutesBefore { get; set; } = new List<int>();
    }
}
