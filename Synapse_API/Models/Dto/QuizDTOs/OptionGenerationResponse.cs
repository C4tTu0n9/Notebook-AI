using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Synapse_API.Models.Dto.QuizDTOs
{
    public class OptionGenerationResponse
    {
        public string OptionKey { get; set; } // Key ("A")
        public string OptionText { get; set; } // Content ("Lựa chọn 1")
        public bool IsCorrect { get; set; }
    }
}
