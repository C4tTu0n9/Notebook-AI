using AutoMapper;
using Synapse_API.Models.Dto.TopicDTOs;
using Synapse_API.Repositories.Course;

namespace Synapse_API.Services.CourseServices
{
    public class TopicService
    {
        private readonly TopicRepository _topicRepository;
        private readonly IMapper _mapper;

        public TopicService(TopicRepository topicRepository, IMapper mapper)
        {
            _topicRepository = topicRepository;
            _mapper = mapper;
        }

        public async Task<List<TopicDto>> GetAllTopics()
        {
            var topics = await _topicRepository.GetAllTopics();
            return _mapper.Map<List<TopicDto>>(topics);
        }

        public async Task<TopicDto> GetTopicById(int id)
        {
            var topic = await _topicRepository.GetTopicById(id);
            if (topic == null)
            {
                return null;
            }
            return _mapper.Map<TopicDto>(topic);
        }

        public async Task<TopicDto> CreateTopic(CreateTopicDto topicDto)
        {
            var topic = _mapper.Map<Models.Entities.Topic>(topicDto);
            topic = await _topicRepository.AddTopic(topic);
            return _mapper.Map<TopicDto>(topic);
        }

        public async Task<TopicDto> UpdateTopic(int id, UpdateTopicDto topicDto)
        {
            var topic = await _topicRepository.GetTopicById(id);
            if (topic == null)
            {
                return null;
            }
            if ((topic.DocumentUrl != string.Empty || topic.DocumentUrl != null)
                && (topicDto.DocumentUrl == string.Empty))
            {
                topicDto.DocumentUrl = topic.DocumentUrl;
            }
            _mapper.Map(topicDto, topic);
            topic = await _topicRepository.UpdateTopic(topic);
            return _mapper.Map<TopicDto>(topic);
        }
        public async Task<TopicDto> DeleteTopic(int id)
        {
            var topic = await _topicRepository.GetTopicById(id);
            if (topic == null)
            {
                return null;
            }
            await _topicRepository.DeleteTopic(topic);
            return _mapper.Map<TopicDto>(topic);
        }
        public async Task<List<TopicDto>> GetTopicByCourseId(int courseId)
        {
            var topics = await _topicRepository.GetTopicByCourseId(courseId);
            return _mapper.Map<List<TopicDto>>(topics);
        }
    }
}
