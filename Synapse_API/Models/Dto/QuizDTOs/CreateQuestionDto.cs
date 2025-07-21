namespace Synapse_API.Models.Dto.QuizDTOs
{
    public class CreateQuestionDto
    {
        public int QuizID { get; set; }
        public string QuestionText { get; set; }
        public string Explanation { get; set; }
    }
}
