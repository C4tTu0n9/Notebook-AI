using Microsoft.AspNetCore.Mvc;
using Synapse_API.Models.Dto.QuizDTOs;
using Synapse_API.Services.CourseServices;
using Synapse_API.Services.CourseServices.QuizServices;
using Synapse_API.Utils;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Synapse_API.Services;
using DocumentFormat.OpenXml.Drawing.Charts;

namespace Synapse_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class QuizController : ControllerBase
    {
        private readonly QuizService _quizService;
        private readonly QuestionService _questionService;
        private readonly OptionService _optionService;
        private readonly TopicService _topicService;
        private readonly QuizAttemptService _quizAttemptService;
        private readonly UserService _userService;

        public QuizController(QuizService quizService, QuestionService questionService, OptionService optionService, TopicService topicService, QuizAttemptService quizAttemptService, UserService userService)
        {
            _quizService = quizService;
            _questionService = questionService;
            _optionService = optionService;
            _topicService = topicService;
            _quizAttemptService = quizAttemptService;
            _userService = userService;
        }

        [HttpPost("create-new-quiz")]
        public async Task<IActionResult> CreateNewQuiz(
            [FromBody] CreateQuizDto createQuizDto)
        {
            var result = await _quizService.CreateQuiz(createQuizDto);

            if (result == null)
                return BadRequest("Failed to create quiz");

            return Ok(result);
        }

        [HttpPost("create-new-question")]
        public async Task<IActionResult> CreateNewQuestion(
            [FromBody] CreateQuestionDto createQuestionDto)
        {
            var result = await _questionService.CreateQuestion(createQuestionDto);

            if (result == null)
                return BadRequest("Failed to create quiz");

            return Ok(result);
        }

        [HttpPost("create-new-option")]
        public async Task<IActionResult> CreateNewOption(
            [FromBody] CreateOptionDto createOptionDto)
        {
            var result = await _optionService.CreateOption(createOptionDto);

            if (result == null)
                return BadRequest("Failed to create quiz");

            return Ok(result);
        }


        [HttpPost("generate-quiz/for-topic/{topicID}")]
        public async Task<IActionResult> GenerateQuiz(
            [FromBody] QuizGenerationRequest quizGenerationRequest,
            int topicID)
        {
            var topic = await _topicService.GetTopicById(topicID);
            if (topic == null)
            {
                return BadRequest("Topic not found!");
            }
            if (topic.DocumentUrl == null || topic.DocumentUrl == string.Empty)
            {
                return BadRequest("Topic does not have a document URL!");
            }
            var result = await _quizService
            .GenerateQuizFromDocument(  //TEST -> (dùng thật bỏ TEST)
                topic.DocumentUrl,
                quizGenerationRequest.QuizTitle,
                quizGenerationRequest.NumberOfQuestions,
                mimeType: MimeTypeHelper.GetMimeType(topic.DocumentUrl),
                quizGenerationRequest.PromptInstruction
            );
            await _quizService.CreateQuizFromAI(result, topicID);

            if (result == null)
                return BadRequest("Failed to generate quiz");

            return Ok(result);
        }

        [HttpPost("generate-a-question")]
        public async Task<IActionResult> GenerateAQuestion(
            [FromQuery] string documentS3Url,
            [FromQuery] string promptInstruction = "")
        {
            var result = await _questionService
            .GenerateAQuestion(
                documentS3Url,
                mimeType: MimeTypeHelper.GetMimeType(documentS3Url),
                promptInstruction
            );
            await _questionService.CreateQuestionFromAI(result);

            if (result == null)
                return BadRequest("Failed to generate quiz");

            return Ok(result);
        }

        [HttpGet("get-quiz-by-topic-id/{topicID}")]
        public async Task<IActionResult> GetQuizByTopicId(int topicID)
        {
            var result = await _quizService.GetQuizByTopicId(topicID);
            return Ok(result);
        }

        // ===== QUIZ ATTEMPT ENDPOINTS =====

        [HttpGet("start-quiz/{quizID}")]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> StartQuiz(int quizID)
        {
            try
            {
                //var userId = _userService.GetMyUserId(User);

                //// Kiểm tra user đã làm quiz này chưa (tùy chọn - có thể cho phép làm lại)
                //var hasAttempted = await _quizAttemptService.HasUserAttemptedQuizAsync(userId, quizID);
                //if (hasAttempted)
                //{
                //    //return BadRequest("Bạn đã làm bài quiz này rồi!");
                //}

                var quiz = await _quizService.GetQuizWithQuestionsAsync(quizID);
                if (quiz == null)
                    return NotFound("Quiz không tồn tại!");

                return Ok(quiz);
            }
            catch (Exception ex)
            {
                return BadRequest($"Lỗi: {ex.Message}");
            }
        }

        [HttpPost("submit-quiz")]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> SubmitQuiz([FromBody] SubmitQuizRequest request)
        {
            try
            {
                var userId = _userService.GetMyUserId(User);
                var result = await _quizAttemptService.SubmitQuizAsync(userId, request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest($"Lỗi: {ex.Message}");
            }
        }

        [HttpGet("quiz-result/{attemptID}")]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> GetQuizResult(int attemptID)
        {
            try
            {
                var result = await _quizAttemptService.GetQuizAttemptResultAsync(attemptID);
                if (result == null)
                    return NotFound("Không tìm thấy kết quả bài quiz!");

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest($"Lỗi: {ex.Message}");
            }
        }

        [HttpGet("my-quiz-history")]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> GetMyQuizHistory()
        {
            try
            {
                var userId = _userService.GetMyUserId(User);
                var result = await _quizAttemptService.GetUserAttemptHistoryAsync(userId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest($"Lỗi: {ex.Message}");
            }
        }
        [HttpDelete("{quizID}")]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> DeleteQuiz(int quizID)
        {
            var quiz = await _quizService.GetQuizById(quizID);
            if (quiz == null)
                return NotFound("Quiz không tồn tại!");
            var result = await _quizService.DeleteQuiz(quizID);
            return Ok(result);
        }
        [HttpGet("get-quiz-result-by-quiz-id/{quizID}")]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> GetQuizResultByQuizID(int quizID)
        {
            try
            {
                var quiz = await _quizService.GetQuizById(quizID);
                if (quiz == null)
                    return NotFound("Quiz không tồn tại!");
                var result = await _quizAttemptService.GetQuizResultByQuizID(quizID);
                return Ok(result);
            }
            catch (System.Exception ex)
            {
                return BadRequest($"Lỗi: {ex.Message}");
            }
        }

    }
}
