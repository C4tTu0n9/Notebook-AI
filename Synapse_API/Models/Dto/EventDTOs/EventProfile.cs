using Synapse_API.Models.Entities;

namespace Synapse_API.Models.Dto.EventDTOs
{
    public class EventProfile : AutoMapper.Profile
    {
        public EventProfile()
        {
            CreateMap<Event, EventDto>()
                .ForMember(dest => dest.EventType, opt => opt.MapFrom(src => src.EventType.ToString()));
            CreateMap<CreateEventDto, Event>()
                .ForMember(dest => dest.EventType, opt => opt.MapFrom(src => Enum.Parse<Enums.EventType>(src.EventType)));
            CreateMap<UpdateEventDto, Event>()
                .ForMember(dest => dest.EventType, opt => opt.MapFrom(src => Enum.Parse<Enums.EventType>(src.EventType)));
        }
    }
}
