using AutoMapper;
using Synapse_API.Models.Entities;

namespace Synapse_API.Models.Dto.CourseDTOs
{
    public class CourseProfile : Profile
    {
        public CourseProfile()
        {
            CreateMap<Course, CourseResponseDto>();
        }
    }
}
