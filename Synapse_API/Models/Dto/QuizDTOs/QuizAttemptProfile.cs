using AutoMapper;
using Synapse_API.Models.Entities;

namespace Synapse_API.Models.Dto.QuizDTOs
{
    public class QuizAttemptProfile : Profile
    {
        public QuizAttemptProfile()
        {
            // Quiz với Questions mapping
            CreateMap<Quiz, QuizWithQuestionsDto>()
                .ForMember(dest => dest.Questions, opt => opt.MapFrom(src => src.Questions));

            // Question với Options mapping (ẩn IsCorrect)
            CreateMap<Question, QuestionWithOptionsDto>()
                .ForMember(dest => dest.Options, opt => opt.MapFrom(src => src.Options));

            // Option mapping (không bao gồm IsCorrect để tránh leak đáp án)
            CreateMap<Option, OptionWithoutCorrectDto>();

            // UserQuizAttempt mapping
            CreateMap<UserQuizAttempt, QuizAttemptResponseDto>()
                .ForMember(dest => dest.QuestionResults, opt => opt.Ignore()); // Sẽ được xử lý riêng trong service

            // UserAnswer mapping
            CreateMap<UserAnswer, QuestionResultDto>()
                .ForMember(dest => dest.QuestionText, opt => opt.MapFrom(src => src.Question.QuestionText))
                .ForMember(dest => dest.CorrectOption, opt => opt.MapFrom(src => 
                    src.Question.Options.FirstOrDefault(o => o.IsCorrect).OptionKey))
                .ForMember(dest => dest.Explanation, opt => opt.MapFrom(src => src.Question.Explanation));
        }
    }
} 