using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Synapse_API.Models.Entities
{
    public class UserAnswer
    {
        [Key]
        public int AnswerID { get; set; }

        [ForeignKey("UserQuizAttempt")]
        public int AttemptID { get; set; }

        [ForeignKey("Question")]
        public int QuestionID { get; set; }

        [StringLength(100)]
        public string? SelectedOption { get; set; }

        public bool? IsCorrect { get; set; }

        public UserQuizAttempt UserQuizAttempt { get; set; }
        public Question Question { get; set; }
    }
}