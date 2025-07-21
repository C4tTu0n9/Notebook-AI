namespace Synapse_API.Models.Dto.QuizDTOs
{
    public class QuizWithQuestionsDto
    {
        public int QuizID { get; set; }
        public int TopicID { get; set; }
        public string QuizTitle { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<QuestionWithOptionsDto> Questions { get; set; }
    }

    public class QuestionWithOptionsDto
    {
        public int QuestionID { get; set; }
        public string QuestionText { get; set; }
        public string Explanation { get; set; }
        public List<OptionWithoutCorrectDto> Options { get; set; }
    }

    public class OptionWithoutCorrectDto
    {
        public int OptionID { get; set; }
        public string OptionKey { get; set; }
        public string OptionText { get; set; }
        // Không bao gồm IsCorrect để tránh leak đáp án cho student
    }
} 