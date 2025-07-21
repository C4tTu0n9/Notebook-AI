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

        public LearningAnalysisController(LearningActivityService learningActivityService, AnalyticsService analyticsService, GeminiService geminiService)
        {
            _learningActivityService = learningActivityService;
            _analyticsService = analyticsService;
            _geminiService = geminiService;

        }



        //[HttpPost("record-learning-activities")]
        //public async Task<IActionResult> RecordLearningData([FromBody] LearningActivityDto dto)
        //{
        //    var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        //    if (string.IsNullOrEmpty(userIdStr))
        //        return Unauthorized("User ID missing from token.");

        //    int userId = int.Parse(userIdStr);

        //        var activityId = await _learningActivityService.RecordLearningActivityAsync(userId, dto);
        //        return Ok(new
        //        {
        //            message = "Learning activity recorded successfully.",
        //            activityId = activityId
        //        });

        //}


        //[HttpGet("learning-activities")]
        //public async Task<IActionResult> GetAllActivities()
        //{
        //    var result = await _learningActivityService.GetAllLearningActivity();
        //    return Ok(result);
        //}


        //[HttpGet("calculate-learning-index/{topicId}")]
        //public async Task<IActionResult> CalculateLearningIndex(int topicId)
        //{
        //    try
        //    {
        //        var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        //        if (string.IsNullOrEmpty(userIdStr))
        //            return Unauthorized("User ID missing from token.");

        //        int userId = int.Parse(userIdStr);

        //        var metrics = await _analyticsService.CalculateAndStoreLearningMetricsByUserAndTopicAsync(userId, topicId);

        //        return Ok(new
        //        {
        //            message = "Learning metric calculated successfully.",
        //            metric = new
        //            {
        //                metrics.UserID,
        //                metrics.TopicID,
        //                metrics.AverageTime,
        //                metrics.CorrectRate,
        //                metrics.TrendScore,
        //                metrics.LastUpdated
        //            }
        //        });
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, new
        //        {
        //            error = "An error occurred while calculating learning index.",
        //            details = ex.Message
        //        });
        //    }
        //}


        [HttpGet("progress-chart")]
        public async Task<IActionResult> GetProgressChart([FromQuery] int topicId, [FromQuery] int month, [FromQuery] int year)
        {
            try
            {
                var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdStr))
                    return Unauthorized("User ID missing from token.");

                int userId = int.Parse(userIdStr);

                // Gọi đến repository thông qua service
                var attempts = await _analyticsService.GetAllUserQuizAttemptsByUserIdAsync(userId, month, year);

                // Lọc theo TopicID (nằm trong Quiz)
                var filtered = attempts
                    .Where(a => a.Quiz?.TopicID == topicId)
                    .OrderBy(a => a.AttemptDate)
                    .Select(a => new
                    {
                        a.AttemptID,
                        a.QuizID,
                        QuizTitle = a.Quiz?.QuizTitle,
                        a.Score,
                        a.AttemptDate,
                        a.Feedback
                    })
                    .ToList();

                return Ok(new
                {
                    userId,
                    topicId,
                    month,
                    year,
                    attempts = filtered
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Failed to retrieve progress chart.", details = ex.Message });
            }
        }



        [HttpGet("weak-topics")]
        public async Task<IActionResult> GetWeakTopics()
        {
            try
            {

                var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdStr))
                    return Unauthorized("User ID missing from token.");

                int userId = int.Parse(userIdStr);

                var weakTopics = await _analyticsService.GetWeakTopicsAsync(userId);

                var result = weakTopics
                    .GroupBy(w => w.TopicID)
                    .Select(g => new
                    {
                        TopicName = g.First().Topic?.TopicName ?? "Unknown",
                        CourseName = g.First().Topic?.Course?.CourseName ?? "Unknown",
                        AverageCorrectRate = Math.Round(g.Average(x => x.CorrectRate ?? 0), 2)
                    }).ToList();

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Failed to detect weak topics.", details = ex.Message });
            }
        }


        [HttpGet("learning-trends")]
        public async Task<IActionResult> GetLearningTrends()
        {
            try
            {
                var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdStr))
                    return Unauthorized("User ID missing from token.");

                int userId = int.Parse(userIdStr);

                var result = await _analyticsService.GetLearningTrendsGroupedAsync(userId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Failed to analyze learning trends.", details = ex.Message });
            }
        }


        [HttpGet("generate-report")]
        public async Task<IActionResult> GenerateLearningReport(
                [FromQuery] string format = "json",
                [FromQuery] int month = 0,
                [FromQuery] int year = 0)
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr))
                return Unauthorized("User ID missing from token.");

            int userId = int.Parse(userIdStr);

            var report = await _analyticsService.GenerateLearningReportAsync(userId, month, year);

            if (format.ToLower() == "pdf")
            {
                var pdfBytes = PdfReportGenerator.GenerateReportPdf(report); // Truyền List<EnhancedLearningReportDto>
                var fileName = $"LearningReport_{DateTime.Now:yyyyMMddHHmmss}.pdf";
                return File(pdfBytes, "application/pdf", fileName);
            }


            return Ok(report); // JSON xem trước
        }



        [HttpGet("compare-goals")]
        public async Task<IActionResult> CompareToGoals([FromQuery] int month, [FromQuery] int year)
        {
            try
            {
                var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdStr))
                    return Unauthorized("User ID missing from token.");

                int userId = int.Parse(userIdStr);

                var comparisons = await _analyticsService.ComparePerformanceToGoalsAsync(userId, month, year);

                return Ok(comparisons);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Failed to compare goals.", details = ex.Message });
            }
        }

        [HttpGet("ai-suggestions")]
        public async Task<IActionResult> GetAISuggestions([FromQuery] int month, [FromQuery] int year)
        {
            try
            {
                var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdStr))
                    return Unauthorized("User ID missing from token.");

                int userId = int.Parse(userIdStr);

                var prompt = await _analyticsService.BuildLearningSuggestionPromptAsync(userId,month,year);
                var suggestion = await _geminiService.GenerateContent(prompt);

                var cleanSuggestion = suggestion
                .Replace("\r\n", " ")
                .Replace("\n", " ")
                .Replace("\r", " ");

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
