using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Synapse_API.Models.Dto.QuizDTOs
{
    public class QuestionGenerationResponse
    {
        public string QuestionText { get; set; }
        public ICollection<OptionGenerationResponse> Options { get; set; }
        public string Explanation { get; set; }
    }
}
