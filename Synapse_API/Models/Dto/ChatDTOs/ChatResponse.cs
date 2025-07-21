namespace Synapse_API.Models.Dto.ChatDTOs
{
    public class ChatResponse
    {
        public string Answer { get; set; }
        public List<string> SourceDocuments { get; set; }
        public string MessageId { get; set; }

        public List<string> History { get; set; } = new List<string>();
    }
}
