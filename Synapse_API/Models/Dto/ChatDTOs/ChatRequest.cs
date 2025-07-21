namespace Synapse_API.Models.Dto.ChatDTOs
{
    public class ChatRequest
    {
        public string Message { get; set; }
        public int UserId { get; set; }
        public int CourseId { get; set; }
        public int TopicId { get; set; }

        public List<string> History { get; set; } = new List<string>(); 
    }
}
