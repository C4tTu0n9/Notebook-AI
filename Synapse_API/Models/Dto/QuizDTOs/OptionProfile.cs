namespace Synapse_API.Models.Dto.QuizDTOs
{
    public class OptionProfile : AutoMapper.Profile
    {
        public OptionProfile()
        {
            CreateMap<Models.Entities.Option, OptionDto>();
            CreateMap<CreateOptionDto, Models.Entities.Option>();
            CreateMap<Models.Entities.Option, OptionGenerationResponse>();
            CreateMap<OptionGenerationResponse, Models.Entities.Option>();
            // CreateMap<UpdateQuizDto, Models.Entities.Quiz>();
        }
    }
}
