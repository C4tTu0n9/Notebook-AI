using Microsoft.Extensions.Logging;
using Synapse_API.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace Synapse_API.Models.Entities
{
    public class User
    {
        [Key]
        public int UserID { get; set; }

        [Required]
        [StringLength(255)]
        public string Email { get; set; }

        [Required]
        [StringLength(255)]
        public string PasswordHash { get; set; }

        [Required]
        [StringLength(100)]
        public string FullName { get; set; }

        [Required]
        [StringLength(50)]
        public UserRole Role { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        public UserProfile UserProfile { get; set; }
        public ICollection<LearningPath> LearningPaths { get; set; }
        public ICollection<Event> Events { get; set; }
        public ICollection<Token> Tokens { get; set; }
        public ICollection<UserQuizAttempt> UserQuizAttempts { get; set; }
        public ICollection<LearningActivity> LearningActivities { get; set; }
        public ICollection<Goal> Goals { get; set; }
        public ICollection<Subscription> Subscriptions { get; set; }
        public ICollection<Payment> Payments { get; set; }
        public ICollection<TimeEstimation> TimeEstimations { get; set; }
        public ICollection<PerformanceMetric> PerformanceMetrics { get; set; } // Added
        public ICollection<Course> Courses { get; set; }
    }
}
