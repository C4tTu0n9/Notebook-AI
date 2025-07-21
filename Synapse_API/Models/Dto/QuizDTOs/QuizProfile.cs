using Synapse_API.Models.Entities;

namespace Synapse_API.Models.Dto.QuizDTOs
{
    public class QuizProfile : AutoMapper.Profile
    {
        public QuizProfile()
        {
            CreateMap<Models.Entities.Quiz, QuizDto>();
            CreateMap<CreateQuizDto, Models.Entities.Quiz>();
            CreateMap<QuizGenerationResponse, Models.Entities.Quiz>();
            // CreateMap<UpdateQuizDto, Models.Entities.Quiz>();
        }
    }
}
