using AutoMapper;
using Synapse_API.Models.Dto.EventDTOs;
using Synapse_API.Models.Entities;

namespace Synapse_API.Models.Dto.UserDTOs
{
    public class UserMapping : Profile
    {
        public UserMapping()
        {
            // Map enum sang string (UserRole -> string)
            CreateMap<User, UserDto>()
                .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Role.ToString()))
                .ForMember(dest => dest.Events, opt => opt.MapFrom(src => src.Events));
                //.ForMember(dest => dest.LearningPaths, opt => opt.MapFrom(src => src.LearningPaths))
                //.ForMember(dest => dest.UserProfile, opt => opt.MapFrom(src => src.UserProfile));

            // Map các collection liên quan
            CreateMap<Event, EventDto>();
            //CreateMap<LearningPath, LearningPathDto>();
            //CreateMap<UserProfile, UserProfileDto>();

            // ... các mapping khác nếu cần
        }
    }
}
