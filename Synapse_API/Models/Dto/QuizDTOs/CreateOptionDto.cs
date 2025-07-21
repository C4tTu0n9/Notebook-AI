namespace Synapse_API.Models.Dto.QuizDTOs
{
    public class CreateOptionDto
    {
        public int QuestionID { get; set; }
        public string OptionKey { get; set; }
        public string OptionText { get; set; }
        public bool IsCorrect { get; set; }
    }
}
