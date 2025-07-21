using System.ComponentModel.DataAnnotations;

namespace Synapse_API.Models.Dto.CourseDTOs
{
    public class CourseRequestDto
    {
        [Required]
        [StringLength(100)]
        public string CourseName { get; set; }

        public string? Description { get; set; }

        //public DateTime CreatedAt { get; set; } = DateTime.Now;

        //public bool IsActive { get; set; } = true;
    }
}
