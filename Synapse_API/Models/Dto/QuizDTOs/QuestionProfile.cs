using Synapse_API.Models.Entities;

namespace Synapse_API.Models.Dto.QuizDTOs
{
    public class QuestionProfile : AutoMapper.Profile
    {
        public QuestionProfile()
        {
            CreateMap<Question, QuestionDto>();
            CreateMap<CreateQuestionDto, Question>();
            CreateMap<Question, QuestionGenerationResponse>()
                .ForMember(dest => dest.Options, opt => opt.MapFrom(src => src.Options));
            CreateMap<QuestionGenerationResponse, Question>();
            // CreateMap<UpdateQuizDto, Models.Entities.Quiz>();
        }
    }
}
