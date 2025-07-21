using System.ComponentModel.DataAnnotations;

namespace Synapse_API.Models.Dto.TopicDTOs
{
    public class CreateTopicDto
    {
        [Required(ErrorMessage = "Field is required.")]
        public int CourseID { get; set; }
        [Required(ErrorMessage = "Field is required.")]
        [StringLength(100, ErrorMessage = "Title must be less than 100 characters.")]
        public string TopicName { get; set; }
        public string? Description { get; set; }
        public string? DocumentUrl { get; set; }

    }
}
