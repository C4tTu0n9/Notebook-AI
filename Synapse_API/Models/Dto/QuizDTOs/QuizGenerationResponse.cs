namespace Synapse_API.Models.Dto.QuizDTOs
{
    public class QuizGenerationResponse
    {
        public string QuizTitle { get; set; }
        public List<QuestionGenerationResponse> Questions { get; set; }
    }
}
