using AutoMapper;
using Synapse_API.Models.Dto.QuizDTOs;
using Synapse_API.Repositories.Course.Quiz;
using Synapse_API.Services.AIServices;

namespace Synapse_API.Services.CourseServices.QuizServices
{
    public class OptionService
    {
        private readonly GeminiService _geminiService;
        private readonly OptionRepository _optionRepository;
        private readonly IMapper _mapper;

        public OptionService(GeminiService geminiService, OptionRepository optionRepository, IMapper mapper)
        {
            _geminiService = geminiService;
            _optionRepository = optionRepository;
            _mapper = mapper;
        }

        public async Task<OptionDto> CreateOption(CreateOptionDto createOptionDto)
        {
            var optionEntity = _mapper.Map<Models.Entities.Option>(createOptionDto);
            var optionNew = await _optionRepository.CreateOption(optionEntity);
            return _mapper.Map<OptionDto>(optionNew);
        }
    }
}
