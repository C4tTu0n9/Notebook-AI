using System.ComponentModel.DataAnnotations;

namespace Synapse_API.Models.Dto.EventDTOs
{
    public class CreateEventDto
    {
        [Required]
        public int UserID { get; set; }

        [Required(ErrorMessage = "Field is required.")]
        public string EventType { get; set; }

        [Required(ErrorMessage = "Title is required.")]
        [StringLength(100, ErrorMessage = "Title must be less than 100 characters.")]
        public string Title { get; set; }

        public string? Description { get; set; }

        [Required(ErrorMessage = "Start time is required.")]
        [DataType(DataType.DateTime)]
        public DateTime StartTime { get; set; }

        [Required(ErrorMessage = "End time is required.")]
        [DataType(DataType.DateTime)]
        public DateTime EndTime { get; set; }
        public int? Priority { get; set; }
        public int? CourseID { get; set; }
        public int? ParentEventID { get; set; }
    }

}
