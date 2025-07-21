using System.ComponentModel.DataAnnotations;

namespace Synapse_API.Models.Dto.TopicDTOs
{
    public class CreateTopicRequest
    {
        [Required(ErrorMessage = "Course ID is required.")]
        public int CourseID { get; set; }

        [Required(ErrorMessage = "Topic name is required.")]
        [StringLength(100, ErrorMessage = "Title must be less than 100 characters.")]
        public string TopicName { get; set; }

        public string? Description { get; set; }
        public IFormFile? DocumentFile { get; set; }
    }
}
