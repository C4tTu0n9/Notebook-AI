using AutoMapper;
using Mscc.GenerativeAI;
using Synapse_API.Models.Dto.QuizDTOs;
using Synapse_API.Repositories.Course.Quiz;
using Synapse_API.Services.AIServices;
using System.Text.Json;

namespace Synapse_API.Services.CourseServices.QuizServices
{
    public class QuestionService
    {
        private readonly GeminiService _geminiService;
        private readonly QuestionRepository _questionRepository;
        private readonly IMapper _mapper;

        public QuestionService(GeminiService geminiService, QuestionRepository questionRepository, IMapper mapper)
        {
            _geminiService = geminiService;
            _questionRepository = questionRepository;
            _mapper = mapper;
        }


        public async Task<QuestionDto> CreateQuestion(CreateQuestionDto createQuestionDto)
        {
            var questionEntity = _mapper.Map<Models.Entities.Question>(createQuestionDto);
            var questionNew = await _questionRepository.CreateQuestion(questionEntity);
            return _mapper.Map<QuestionDto>(questionNew);
        }

        public async Task<QuestionDto> CreateQuestionFromAI(QuestionGenerationResponse questionGenerationResponse)
        {
            var questionEntity = _mapper.Map<Models.Entities.Question>(questionGenerationResponse);
            questionEntity.QuizID = 1;
            var questionNew = await _questionRepository.CreateQuestion(questionEntity);
            return _mapper.Map<QuestionDto>(questionNew);
        }

        /// <summary>
        /// Tạo một Question từ tài liệu dựa trên URL S3.
        /// </summary>
        public async Task<QuestionGenerationResponse> GenerateAQuestion(
            string documentS3Url,
            string mimeType,
            string promptInstruction = "")
        {
            // Đảm bảo URL hợp lệ
            if (string.IsNullOrEmpty(documentS3Url))
            {
                throw new ArgumentNullException(nameof(documentS3Url), "URL tài liệu không được để trống.");
            }
            string googleFileUri = await _geminiService.UploadS3FileToGoogleFileApi(documentS3Url, mimeType);
            string fullPrompt = $"Based on the content of the document, create a question" +
                                $"{promptInstruction}.";
            var parts = new List<IPart>
            {
                new TextData { Text = fullPrompt },
                new FileData { FileUri = googleFileUri  }
            };
            var request = new GenerateContentRequest(parts);
            var generationConfig = new GenerationConfig
            {
                ResponseMimeType = "application/json", // JSON
                ResponseSchema = new QuestionGenerationResponse() // schema 
            };
            var response = await _geminiService.GenerateContentWithConfig(request, generationConfig);
            if (response?.Text == null)
            {
                throw new Exception($"Không thể tạo question. Lời nhắc bị chặn: {response?.PromptFeedback?.BlockReason}");
            }
            var questionResponse = JsonSerializer.Deserialize<QuestionGenerationResponse>(response.Text, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            return questionResponse;
        }
    }
}
