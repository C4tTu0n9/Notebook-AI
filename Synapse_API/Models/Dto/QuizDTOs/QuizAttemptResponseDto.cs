namespace Synapse_API.Models.Dto.QuizDTOs
{
    public class QuizAttemptResponseDto
    {
        public int AttemptID { get; set; }
        public int UserID { get; set; }
        public int QuizID { get; set; }
        public decimal Score { get; set; }
        public DateTime AttemptDate { get; set; }
        public string Feedback { get; set; }
        public List<QuestionResultDto> QuestionResults { get; set; }
    }

    public class QuestionResultDto
    {
        public int QuestionID { get; set; }
        public string QuestionText { get; set; }
        public string SelectedOption { get; set; }
        public string CorrectOption { get; set; }
        public bool IsCorrect { get; set; }
        public string Explanation { get; set; }
    }
} 