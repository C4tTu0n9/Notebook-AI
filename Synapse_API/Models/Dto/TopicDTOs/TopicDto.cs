using Synapse_API.Models.Entities;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Synapse_API.Models.Dto.TopicDTOs
{
    public class TopicDto
    {
        public int TopicID { get; set; }
        public int CourseID { get; set; }
        public string TopicName { get; set; }
        public string Description { get; set; }
        public string DocumentUrl { get; set; }
    }
}
