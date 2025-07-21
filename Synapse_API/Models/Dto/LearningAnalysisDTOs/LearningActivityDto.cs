using Synapse_API.Models.Enums;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Synapse_API.Models.Dto.LearningAnalysisDTOs
{
    public class LearningActivityDto
    {

        public ActivityType ActivityType { get; set; }

        public DateTime StartTime { get; set; }

        public DateTime EndTime { get; set; }

    }
}
