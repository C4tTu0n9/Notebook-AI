namespace Synapse_API.Models.Dto.QuizDTOs
{
    public class QuizGenerationRequest
    {
        public string QuizTitle { get; set; }
        public int NumberOfQuestions { get; set; }
        public string? PromptInstruction { get; set; }
    }
}
