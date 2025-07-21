using System.ComponentModel.DataAnnotations;

namespace Synapse_API.Models.Entities
{
    public class Course
    {
        [Key]
        public int CourseID { get; set; }

        [Required]
        public int UserID { get; set; }

        [Required]
        [StringLength(100)]
        public string CourseName { get; set; }

        public string? Description { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public bool IsActive { get; set; } = true;

        public ICollection<Topic> Topics { get; set; }
        public User User { get; set; } 
        public ICollection<Event> Events { get; set; }
    }
}