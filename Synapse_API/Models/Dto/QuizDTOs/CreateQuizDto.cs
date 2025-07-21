using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Synapse_API.Models.Dto.QuizDTOs
{
    public class CreateQuizDto
    {
        public int TopicID { get; set; }

        [Required(ErrorMessage = "Field is required.")]
        [StringLength(100)]
        public string QuizTitle { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
