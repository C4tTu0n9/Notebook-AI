using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Synapse_API.Models.Dto.QuizDTOs
{
    public class OptionDto
    {
        public int OptionID { get; set; }
        public int QuestionID { get; set; }
        public string OptionKey { get; set; }
        public string OptionText { get; set; }
        public bool IsCorrect { get; set; }
    }
}
