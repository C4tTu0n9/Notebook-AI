using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Synapse_API.Models.Dto.ChatDTOs;
using Synapse_API.Models.Dto.DocumentDTOs;
using Synapse_API.Services;
using Synapse_API.Services.AIServices;
using Synapse_API.Services.DocumentServices.Interfaces;

namespace Synapse_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatbotController : ControllerBase
    {
        private readonly ChatbotService _chatbotService;
        private readonly IDocumentProcessingService _documentProcessingService;
        private readonly UserService _userService;

        public ChatbotController(ChatbotService chatbotService, IDocumentProcessingService documentProcessingService, UserService userService)
        {
            _chatbotService = chatbotService;
            _documentProcessingService = documentProcessingService;
            _userService = userService;
        }


        // Mặc định sẽ là POST /api/Chatbot
        [HttpPost]
        public async Task<ActionResult<ChatResponse>> AskAiFromInternal([FromBody] ChatRequest request)
        {
            var userId = _userService.GetMyUserId(User); 
            request.UserId = userId;    
            if (string.IsNullOrWhiteSpace(request.Message))
            {
                return BadRequest("Tin nhắn không được để trống.");
            }

            // Gọi dịch vụ chatbot
            var response = await _chatbotService.GetChatResponseAsync(request);

            if (response == null)
            {
                return StatusCode(500, "Có lỗi xảy ra khi xử lý yêu cầu chatbot.");
            }

            return Ok(response);
        }

        // POST /api/Chatbot/upload
        //[HttpPost("upload")] 
        //[Consumes("multipart/form-data")] // Quan trọng để nhận file upload
        //public async Task<IActionResult> ProcessDocFromInternal([FromForm] UploadDocumentRequest request)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return BadRequest(ModelState);
        //    }

        //    string docName = string.IsNullOrEmpty(request.DocumentName) ? request.File.FileName : request.DocumentName;
        //    bool success = await _documentProcessingService.ProcessAndEmbedDocumentAsync(
        //        request.File,
        //        // Chú ý: UserId ở đây cần lấy từ Context của người dùng đã đăng nhập, không nên lấy từ request trực tiếp nếu không xác thực
        //        request.UserId,
        //        docName
        //    );

        //    if (success)
        //    {
        //        return Ok(new { Message = $"Tài liệu '{docName}' đã được tải lên và xử lý thành công qua nội bộ." });
        //    }
        //    else
        //    {
        //        return StatusCode(500, new { Message = "Có lỗi xảy ra khi xử lý tài liệu nội bộ." });
        //    }
        //}

    }
}
