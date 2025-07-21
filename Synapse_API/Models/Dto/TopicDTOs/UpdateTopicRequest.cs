using System.ComponentModel.DataAnnotations;

namespace Synapse_API.Models.Dto.TopicDTOs
{
    public class UpdateTopicRequest
    {
        [Required(ErrorMessage = "Topic name is required.")]
        [StringLength(100, ErrorMessage = "Title must be less than 100 characters.")]
        public string TopicName { get; set; }

        public string? Description { get; set; }
        public IFormFile? DocumentFile { get; set; }
    }
}
