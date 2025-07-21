namespace Synapse_API.Models.Dto.QuizDTOs
{
    public class QuizDto
    {
        public int QuizID { get; set; }
        public int TopicID { get; set; }
        public string QuizTitle { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
