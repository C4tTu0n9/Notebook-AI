using System.ComponentModel.DataAnnotations;

namespace Synapse_API.Models.Dto.CourseDTOs
{
    public class CourseResponseDto
    {
        public int CourseID { get; set; }
        public string CourseName { get; set; }
        public string Description { get; set; }

    }
    public class ApiResult<T>             
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }
    }
}
