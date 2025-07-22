using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Synapse_API.Data;
using Synapse_API.Models.Dto.LearningAnalysisDTOs;
using Synapse_API.Models.Entities;
using Synapse_API.Services;
using Synapse_API.Services.AIServices;
using Synapse_API.Utils;
using System.Security.Claims;

namespace Synapse_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LearningAnalysisController : ControllerBase
    {

        private readonly LearningActivityService _learningActivityService;
        private readonly AnalyticsService _analyticsService;
        private readonly GeminiService _geminiService;
        private readonly UserService _userService;

        public LearningAnalysisController(LearningActivityService learningActivityService, AnalyticsService analyticsService, GeminiService geminiService, UserService userService)
        {
            _learningActivityService = learningActivityService;
            _analyticsService = analyticsService;
            _geminiService = geminiService;
            _userService = userService;
        }

        [Authorize(Roles = "Student")]
        [HttpGet("progress-chart")]
        public async Task<IActionResult> GetProgressChart([FromQuery] int topicId, [FromQuery] int month, [FromQuery] int year)
        {
            try
            {
                // Lấy userId Claims NameIdentifier
                int userId = _userService.GetMyUserId(User);
                // Gọi đến repository thông qua service
                var attempts = await _analyticsService.GetAllUserQuizAttemptsByUserIdAsync(userId, topicId, month, year);

                return Ok(new
                {
                    userId,
                    topicId,
                    month,
                    year,
                    attempts
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Failed to retrieve progress chart.", details = ex.Message });
            }
        }

        [Authorize(Roles = "Student")]
        [HttpGet("weak-topics")]
        public async Task<IActionResult> GetWeakTopics()
        {
            try
            {
                int userId = _userService.GetMyUserId(User);
                var weakTopics = await _analyticsService.GetWeakTopicsAsync(userId);
                return Ok(weakTopics);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Failed to detect weak topics.", details = ex.Message });
            }
        }


        [Authorize(Roles = "Student")]
        [HttpGet("learning-trends")]
        public async Task<IActionResult> GetLearningTrends()
        {
            try
            {
                int userId = _userService.GetMyUserId(User);
                var result = await _analyticsService.GetLearningTrendsGroupedAsync(userId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Failed to analyze learning trends.", details = ex.Message });
            }
        }

        [Authorize(Roles = "Student")]
        [HttpGet("generate-report")]
        public async Task<IActionResult> GenerateLearningReport(
                [FromQuery] string format = "json",
                [FromQuery] int month = 0,
                [FromQuery] int year = 0)
        {
            int userId = _userService.GetMyUserId(User);
            var report = await _analyticsService.GenerateLearningReportAsync(userId, month, year);

            if (format.ToLower() == "pdf")
            {
                var pdfBytes = PdfReportGenerator.GenerateReportPdf(report); // Truyền List<EnhancedLearningReportDto>
                var fileName = $"LearningReport_{DateTime.Now:yyyyMMddHHmmss}.pdf";
                return File(pdfBytes, "application/pdf", fileName);
            }


            return Ok(report); // JSON xem trước
        }


        [Authorize(Roles = "Student")]
        [HttpGet("ai-suggestions")]
        public async Task<IActionResult> GetAISuggestions([FromQuery] int month, [FromQuery] int year)
        {
            try
            {
                int userId = _userService.GetMyUserId(User);
                var prompt = await _analyticsService.BuildLearningSuggestionPromptAsync(userId,month,year);

                string suggestion;

                if (prompt.Equals("No data available for this student during the selected period. Please analyze manually."))
                {
                    suggestion = prompt;
                }
                else
                {
                    suggestion = await _geminiService.GenerateContent(prompt);
                }
                //var cleanSuggestion = suggestion
                //.Replace("\r\n", " ")
                //.Replace("\n", " ")
                //.Replace("\r", " ");
                return Ok(new
                {
                    userId,
                    month,
                    year,
                    suggestions = suggestion
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "AI suggestion failed", details = ex.Message });
            }
        }




    }
}
