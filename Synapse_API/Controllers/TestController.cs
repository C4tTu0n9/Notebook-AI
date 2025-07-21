using Microsoft.AspNetCore.Mvc;
using Synapse_API.Services;
using Synapse_API.Services.AIServices;
using Synapse_API.Services.AmazonServices;
using Synapse_API.Services.DatabaseServices;
using Synapse_API.Utils;

namespace Synapse_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : ControllerBase
    {
        private readonly GeminiService _geminiService;
        private readonly MyS3Service _myS3Service;
        private readonly EmailService _emailService;
        private readonly RedisService _redisService;
        private readonly UserService _userService;

        public TestController(GeminiService geminiService, MyS3Service myS3Service, EmailService emailService, RedisService redisService,
            UserService userService)
        {
            _geminiService = geminiService;
            _myS3Service = myS3Service;
            _emailService = emailService;
            _redisService = redisService;
            _userService = userService;
        }
        //test gemini
        [HttpPost("ask")]
        public async Task<IActionResult> GenerateText([FromBody] string prompt)
        {
            var result = await _geminiService.GenerateContent(prompt);
            return Ok(result);
        }
        //test s3
        [HttpGet("create-bucket")]
        public async Task<IActionResult> CreateBucket()
        {
            try
            {
                await _myS3Service.CreateNewBucketAsync();
                return Ok("Bucket created successfully");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error creating bucket: {ex.Message}");
            }
        }
        [HttpPost("upload")]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file uploaded");
            }
            //var email = _userService.GetMyEmail(User);
            var email = "tuong0505ht@gmail.com"; // test
            var keyName = FileNameHelper.SetDocumentName(email, file.FileName);
            var url = await _myS3Service.UploadObjectAsync(file, keyName);

            return Ok(new { url });
        }
        [HttpGet("download")]
        public async Task<IActionResult> Download(string key)
        {
            var response = await _myS3Service.GetObjectAsync(key);
            return File(response.ResponseStream, response.Headers["Content-Type"], key);
        }
        [HttpGet("list-objects-all")]
        public async Task<IActionResult> ListObjectsAll()
        {
            var objects = await _myS3Service.ListObjectsAsync();
            return Ok(objects);
        }
        [HttpGet("delete-object")]
        public async Task<IActionResult> DeleteObject(string keyName)
        {
            await _myS3Service.DeleteObjectAsync(keyName);
            return Ok();
        }
        //test ses
        [HttpPost]
        public async Task<IActionResult> SendMail(string to, string title, string body)
        {
            await _emailService.SendEmailAsync(to, title, body);
            return Ok("Gui ok roi nhe!");
        }
        //test redis
        [HttpPost("set-key-redis")]
        public async Task<IActionResult> SetValue(string key, string value)
        {
            await _redisService.SetAsync(key, value);
            return Ok("Đã lưu vào Redis!");
        }

        [HttpGet("get-key-redis")]
        public async Task<IActionResult> GetValue(string key)
        {
            var value = await _redisService.GetAsync(key);
            return Ok(value.ToString());
        }
    }
}
