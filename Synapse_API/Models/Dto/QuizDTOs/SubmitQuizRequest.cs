namespace Synapse_API.Models.Dto.QuizDTOs
{
    public class SubmitQuizRequest
    {
        public int QuizID { get; set; }
        public List<UserAnswerDto> UserAnswers { get; set; }
    }

    public class UserAnswerDto
    {
        public int QuestionID { get; set; }
        public string SelectedOption { get; set; } // "A", "B", "C", "D"
    }
} 