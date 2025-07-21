using AutoMapper;

namespace Synapse_API.Models.Dto.TopicDTOs
{
    public class TopicProfile : Profile
    {
        public TopicProfile()
        {
            CreateMap<CreateTopicDto, Models.Entities.Topic>();
            CreateMap<Models.Entities.Topic, TopicDto>();
            CreateMap<UpdateTopicDto, Models.Entities.Topic>();
        }
    }
}
