using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Synapse_API.Models.Entities
{
    public class Question
    {
        [Key]
        public int QuestionID { get; set; }

        [ForeignKey("Quiz")]
        public int QuizID { get; set; }

        [Required]
        [StringLength(255)]
        public string QuestionText { get; set; }

        public ICollection<Option> Options { get; set; }

        [Required]
        public string Explanation { get; set; } // Giải thích vì sao đúng/sai

        public Quiz Quiz { get; set; }
        public ICollection<UserAnswer> UserAnswers { get; set; }
    }
}