using AutoMapper;
using Mscc.GenerativeAI;
using Synapse_API.Models.Dto.QuizDTOs;
using Synapse_API.Repositories.Course.Quiz;
using Synapse_API.Services.AIServices;
using System.Text.Json;

namespace Synapse_API.Services.CourseServices.QuizServices
{
    public class QuizService
    {
        private readonly GeminiService _geminiService;
        private readonly QuizRepository _quizRepository;
        private readonly IMapper _mapper;

        public QuizService(GeminiService geminiService, QuizRepository quizRepository, IMapper mapper)
        {
            _geminiService = geminiService;
            _quizRepository = quizRepository;
            _mapper = mapper;
        }
        public async Task<QuizDto> CreateQuiz(CreateQuizDto createQuizDto)
        {
            var quizEntity = _mapper.Map<Models.Entities.Quiz>(createQuizDto);
            var quizNew = await _quizRepository.CreateQuiz(quizEntity);
            return _mapper.Map<QuizDto>(quizNew);
        }

        public async Task<QuizDto> CreateQuizFromAI(QuizGenerationResponse quizGenerationResponse, int topicID)
        {
            var quizEntity = _mapper.Map<Models.Entities.Quiz>(quizGenerationResponse);
            quizEntity.TopicID = topicID;
            var quizNew = await _quizRepository.CreateQuiz(quizEntity);
            return _mapper.Map<QuizDto>(quizNew);
        }

        /// <summary>
        /// Tạo một bài kiểm tra trắc nghiệm từ tài liệu dựa trên URL S3.
        /// </summary>
        /// <param name="documentS3Url">URL của tài liệu trên AWS S3.</param>
        /// <param name="quizTitle">Tiêu đề mong muốn cho bài kiểm tra.</param>
        /// <param name="numberOfQuestions">Số lượng câu hỏi cần tạo.</param>
        /// <param name="promptInstruction">Hướng dẫn bổ sung cho AI về cách tạo câu hỏi (ví dụ: "Tạo câu hỏi dễ hiểu", "Tập trung vào phần X").</param>
        /// <returns>Đối tượng QuizGenerationResponse chứa tiêu đề và các câu hỏi.</returns>
        /// <exception cref="HttpRequestException">Ném ra nếu yêu cầu API thất bại.</exception>
        /// <exception cref="JsonException">Ném ra nếu không thể giải mã phản hồi JSON.</exception>
        public async Task<QuizGenerationResponse> GenerateQuizFromDocument(
            string documentS3Url,
            string quizTitle,
            int numberOfQuestions,
            string mimeType,
            string promptInstruction = "")
        {
            // Đảm bảo URL hợp lệ
            if (string.IsNullOrEmpty(documentS3Url))
            {
                throw new ArgumentNullException(nameof(documentS3Url), "URL tài liệu không được để trống.");
            }
            // Gọi phương thức riêng để tải tệp lên Google File API
            string googleFileUri = await _geminiService.UploadS3FileToGoogleFileApi(documentS3Url, mimeType);

            // Tạo lời nhắc cho AI
            string fullPrompt = $"Based on the content of the document, create a multiple choice test with quizTitle {quizTitle} containing {numberOfQuestions} questions. " +
                                $"{promptInstruction}.";

            // Chuẩn bị các phần nội dung cho yêu cầu AI
            var parts = new List<IPart>
            {
                new TextData { Text = fullPrompt },
                new FileData { FileUri = googleFileUri  } // Sử dụng FileData với FileUri cho URL S3
            };

            // Tạo yêu cầu GenerateContent
            var request = new GenerateContentRequest(parts);

            // Cấu hình để AI trả về JSON có cấu trúc
            var generationConfig = new GenerationConfig
            {
                ResponseMimeType = "application/json", // Yêu cầu đầu ra là JSON
                ResponseSchema = new QuizGenerationResponse() // Cung cấp schema để AI tuân theo cấu trúc này
            };

            // Gửi yêu cầu đến API Gemini
            var response = await _geminiService.GenerateContentWithConfig(request, generationConfig);

            if (response?.Text == null)
            {
                // Xử lý trường hợp không có phản hồi văn bản hoặc có lỗi
                throw new Exception($"Không thể tạo Quiz. Lời nhắc bị chặn: {response?.PromptFeedback?.BlockReason}");
            }
            // Giải mã phản hồi JSON thành đối tượng C#
            var quizResponse = JsonSerializer.Deserialize<QuizGenerationResponse>(response.Text, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            return quizResponse;
        }


        public async Task<QuizGenerationResponse> GenerateQuizTEST(
            string documentS3Url,
            string quizTitle,
            int numberOfQuestions,
            string mimeType,
            string promptInstruction = "")
        {
            return JsonSerializer.Deserialize<QuizGenerationResponse>(SampleQuizGenerationResponse(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }

        public string SampleQuizGenerationResponse()
        {
            return @"
            {
              ""quizTitle"": ""MLN"",
              ""questions"": [
                {
                  ""questionText"": ""Theo tài liệu, chủ nghĩa Marx-Lenin là thuật ngữ chính trị để chỉ học thuyết do Karl Marx và Friedrich Engels sáng lập và được ai phát triển kế thừa?"",
                  ""options"": [
                    {
                      ""optionKey"": ""A"",
                      ""optionText"": ""Iosif Vissarionovich Stalin"",
                      ""isCorrect"": false
                    },
                    {
                      ""optionKey"": ""B"",
                      ""optionText"": ""Vladimir Ilyich Lenin"",
                      ""isCorrect"": true
                    },
                    {
                      ""optionKey"": ""C"",
                      ""optionText"": ""Mao Trạch Đông"",
                      ""isCorrect"": false
                    },
                    {
                      ""optionKey"": ""D"",
                      ""optionText"": ""Leon Trotsky"",
                      ""isCorrect"": false
                    }
                  ],
                  ""explanation"": ""Theo đoạn văn, chủ nghĩa Marx-Lenin là học thuyết do Karl Marx và Friedrich Engels sáng lập và được Vladimir Ilyich Lenin phát triển kế thừa. Iosif Vissarionovich Stalin là người đã định nghĩa thuật ngữ này, còn Mao Trạch Đông và Trotsky là những nhân vật có học thuyết khác trong chủ nghĩa cộng sản.""
                },
                {
                  ""questionText"": ""Theo tài liệu, chủ nghĩa Marx-Lenin là thuật ngữ chính trị để chỉ học thuyết do Karl Marx và Friedrich Engels sáng lập và được ai phát triển kế thừa?"",
                  ""options"": [
                    {
                      ""optionKey"": ""A"",
                      ""optionText"": ""Iosif Vissarionovich Stalin"",
                      ""isCorrect"": false
                    },
                    {
                      ""optionKey"": ""B"",
                      ""optionText"": ""Vladimir Ilyich Lenin"",
                      ""isCorrect"": true
                    },
                    {
                      ""optionKey"": ""C"",
                      ""optionText"": ""Mao Trạch Đông"",
                      ""isCorrect"": false
                    },
                    {
                      ""optionKey"": ""D"",
                      ""optionText"": ""Leon Trotsky"",
                      ""isCorrect"": false
                    }
                  ],
                  ""explanation"": ""Theo đoạn văn, chủ nghĩa Marx-Lenin là học thuyết do Karl Marx và Friedrich Engels sáng lập và được Vladimir Ilyich Lenin phát triển kế thừa. Iosif Vissarionovich Stalin là người đã định nghĩa thuật ngữ này, còn Mao Trạch Đông và Trotsky là những nhân vật có học thuyết khác trong chủ nghĩa cộng sản.""
                },
                {
                  ""questionText"": ""Theo tài liệu, chủ nghĩa Marx-Lenin là thuật ngữ chính trị để chỉ học thuyết do Karl Marx và Friedrich Engels sáng lập và được ai phát triển kế thừa?"",
                  ""options"": [
                    {
                      ""optionKey"": ""A"",
                      ""optionText"": ""Iosif Vissarionovich Stalin"",
                      ""isCorrect"": false
                    },
                    {
                      ""optionKey"": ""B"",
                      ""optionText"": ""Vladimir Ilyich Lenin"",
                      ""isCorrect"": true
                    },
                    {
                      ""optionKey"": ""C"",
                      ""optionText"": ""Mao Trạch Đông"",
                      ""isCorrect"": false
                    },
                    {
                      ""optionKey"": ""D"",
                      ""optionText"": ""Leon Trotsky"",
                      ""isCorrect"": false
                    }
                  ],
                  ""explanation"": ""Theo đoạn văn, chủ nghĩa Marx-Lenin là học thuyết do Karl Marx và Friedrich Engels sáng lập và được Vladimir Ilyich Lenin phát triển kế thừa. Iosif Vissarionovich Stalin là người đã định nghĩa thuật ngữ này, còn Mao Trạch Đông và Trotsky là những nhân vật có học thuyết khác trong chủ nghĩa cộng sản.""
                }
             ]
            }";
        }

        public async Task<List<QuizDto>> GetQuizByTopicId(int topicId)
        {
            var quiz = await _quizRepository.GetQuizByTopicId(topicId);
            return _mapper.Map<List<QuizDto>>(quiz);
        }

        public async Task<QuizWithQuestionsDto?> GetQuizWithQuestionsAsync(int quizId)
        {
            var quiz = await _quizRepository.GetQuizWithQuestionsAsync(quizId);
            if (quiz == null) return null;
            
            return _mapper.Map<QuizWithQuestionsDto>(quiz);
        }

        public async Task<bool> DeleteQuiz(int quizID)
        {
            var quiz = await _quizRepository.GetQuizById(quizID);
            if (quiz == null) return false;
            await _quizRepository.DeleteQuiz(quizID);
            return true;
        }
        public async Task<QuizDto?> GetQuizById(int quizID)
        {
            var quiz = await _quizRepository.GetQuizById(quizID);
            if (quiz == null) return null;
            return _mapper.Map<QuizDto>(quiz);
        }

    }
}