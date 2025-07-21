using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Synapse_API.Models.Entities
{
    public class UserQuizAttempt
    {
        [Key]
        public int AttemptID { get; set; }

        [ForeignKey("User")]
        public int UserID { get; set; }

        [ForeignKey("Quiz")]
        public int QuizID { get; set; }

        public decimal? Score { get; set; }

        public DateTime AttemptDate { get; set; } = DateTime.Now;

        [StringLength(255)]
        public string Feedback { get; set; }

        public User User { get; set; }
        public Quiz Quiz { get; set; }
        public ICollection<UserAnswer> UserAnswers { get; set; }
    }
}