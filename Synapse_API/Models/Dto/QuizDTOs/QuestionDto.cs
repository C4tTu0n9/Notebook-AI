using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Synapse_API.Models.Dto.QuizDTOs
{
    public class QuestionDto
    {
        public int QuestionID { get; set; }
        public int QuizID { get; set; }
        public string QuestionText { get; set; }

        public ICollection<OptionDto> Options { get; set; }
        public string Explanation { get; set; }
    }
}
