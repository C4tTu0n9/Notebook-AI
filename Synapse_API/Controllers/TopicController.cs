using Microsoft.AspNetCore.Mvc;
using Synapse_API.Services;
using Synapse_API.Services.CourseServices;
using Synapse_API.Models.Dto.TopicDTOs;
using Synapse_API.Services.AmazonServices;
using Microsoft.AspNetCore.Authorization;
using Synapse_API.Services.DocumentServices.Interfaces;

namespace Synapse_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TopicController : ControllerBase
    {
        private readonly TopicService _topicService;
        private readonly UserService _userService;
        private readonly MyS3Service _myS3Service;
        private readonly IDocumentProcessingService _documentProcessingService;

        public TopicController(TopicService topicService, UserService userService, MyS3Service myS3Service, IDocumentProcessingService documentProcessingService)
        {
            _topicService = topicService;
            _userService = userService;
            _myS3Service = myS3Service;
            _documentProcessingService = documentProcessingService;
        }

        [HttpGet()]
        public async Task<IActionResult> GetAllTopics()
        {
            var topics = await _topicService.GetAllTopics();
            return Ok(topics);
        }
        [Authorize(Roles = "Student")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetTopicById(int id)
        {
            var topic = await _topicService.GetTopicById(id);
            if (topic == null)
            {
                return NotFound("Topic not found");
            }
            return Ok(topic);
        }
        [Authorize(Roles = "Student")]
        [HttpPost()]
        public async Task<IActionResult> CreateTopic([FromForm] CreateTopicRequest topicRequest)
        {
            string fileUrl = string.Empty;
            if (topicRequest.DocumentFile != null)
            {
                var allowedExtensions = new[] { ".pdf", ".doc", ".docx", ".txt" };
                var ext = Path.GetExtension(topicRequest.DocumentFile.FileName).ToLower();

                if (!allowedExtensions.Contains(ext))
                    return BadRequest("Invalid file type. Only PDF, DOC, DOCX, and TXT are allowed.");

                // Lưu file lên S3
                var myEmail = _userService.GetMyEmail(User);
                var fileName = Utils.FileNameHelper.SetDocumentName(myEmail, topicRequest.DocumentFile.FileName);
                fileUrl = await _myS3Service.UploadObjectAsync(topicRequest.DocumentFile, fileName);


            }
            try
            {
                var topicDto = new CreateTopicDto
                {
                    CourseID = topicRequest.CourseID,
                    TopicName = topicRequest.TopicName,
                    Description = topicRequest.Description,
                    DocumentUrl = fileUrl
                };
                var topic = await _topicService.CreateTopic(topicDto);

                // nhúng dữ liêu từ tài liệu vào Qdrant
                var userId = _userService.GetMyUserId(User);
                string docName = string.IsNullOrEmpty(topicRequest.TopicName) ? topicRequest.DocumentFile.FileName : topicRequest.TopicName;
                bool success = await _documentProcessingService.ProcessAndEmbedDocumentAsync(
                    topicRequest.DocumentFile,
                    userId,
                    docName,
                    topicRequest.CourseID,
                    topic.TopicID
                );
                return Ok(topic);
            }
            catch (System.Exception)
            {
                return BadRequest("Failed to create topic");
            }
        }

        [Authorize(Roles = "Student")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTopic(int id, UpdateTopicRequest topicRequest)
        {
            string fileUrl = string.Empty;
            if (topicRequest.DocumentFile != null)
            {
                var allowedExtensions = new[] { ".pdf", ".doc", ".docx", ".txt" };
                var ext = Path.GetExtension(topicRequest.DocumentFile.FileName).ToLower();

                if (!allowedExtensions.Contains(ext))
                    return BadRequest("Invalid file type. Only PDF, DOC, DOCX, and TXT are allowed.");

                // Lưu file lên S3
                var myEmail = _userService.GetMyEmail(User);
                var fileName = Utils.FileNameHelper.SetDocumentName(myEmail, topicRequest.DocumentFile.FileName);
                fileUrl = await _myS3Service.UploadObjectAsync(topicRequest.DocumentFile, fileName);
            }
            try
            {
                var topicDto = new UpdateTopicDto
                {
                    TopicName = topicRequest.TopicName,
                    Description = topicRequest.Description,
                    DocumentUrl = fileUrl
                };
                var topic = await _topicService.UpdateTopic(id, topicDto);

                // nhúng dữ liêu từ tài liệu vào Qdrant
                var userId = _userService.GetMyUserId(User);
                string docName = string.IsNullOrEmpty(topicRequest.TopicName) ? topicRequest.DocumentFile.FileName : topicRequest.TopicName;
                bool success = await _documentProcessingService.ProcessAndEmbedDocumentAsync(
                    topicRequest.DocumentFile,
                    userId,
                    docName,
                    topic.CourseID,
                    topic.TopicID
                );
                return Ok(topic);
            }
            catch (System.Exception)
            {
                return BadRequest("Failed to update topic");
            }
        }

        [Authorize(Roles = "Student")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTopic(int id)
        {
            try
            {
                var topic = await _topicService.DeleteTopic(id);
                return Ok(topic);
            }
            catch (System.Exception)
            {
                return BadRequest("Failed to delete topic");
            }
        }
    }
}

