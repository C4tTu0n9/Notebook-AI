using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Synapse_API.Models.Entities
{
    public class Quiz
    {

        [Key]
        public int QuizID { get; set; }

        [ForeignKey("Topic")]
        public int TopicID { get; set; }

        [Required]
        [StringLength(100)]
        public string QuizTitle { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public Topic Topic { get; set; }
        public ICollection<Question> Questions { get; set; }
        public ICollection<UserQuizAttempt> UserQuizAttempts { get; set; }
    }
}