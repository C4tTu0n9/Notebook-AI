using System.ComponentModel.DataAnnotations;

namespace Synapse_API.Models.Dto.CourseDTOs
{
    public class UpdateCourseDto
    {
        [Required(ErrorMessage = "Field is required.")]
        [StringLength(100, ErrorMessage = "Course Name must be less than 100 characters.")]
        public string CourseName { get; set; }

        public string Description { get; set; }
    }
}
