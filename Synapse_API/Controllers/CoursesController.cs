using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Synapse_API.Services;
using Synapse_API.Services.CourseServices;
using Synapse_API.Models.Dto.CourseDTOs;

namespace Synapse_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CoursesController : ControllerBase
    {
        private readonly CourseService _courseService;
        private readonly UserService _userService;
        private readonly TopicService _topicService;

        public CoursesController(CourseService courseService, UserService userService, TopicService topicService)
        {
            _courseService = courseService;
            _userService = userService;
            _topicService = topicService;
        }

        //[Authorize(Roles = "Student")]
        [HttpGet("get-all-course")]
        public async Task<IActionResult> GetAllCourseAsync()
        {
            var course = await _courseService.GetAllCoursesAsync();
            return Ok(course);
        }

        [Authorize(Roles = "Student")]
        [HttpGet("get-my-course")]
        public async Task<IActionResult> GetMyCourseAsync()
        {
            int userId = _userService.GetMyUserId(User);
            var events = await _courseService.GetCourseByUserId(userId);

            if (!events.Any())
            {
                return NotFound($"No events found for student with ID {userId}.");
            }
            return Ok(events);
        }

        [Authorize(Roles = "Student")]
        [HttpGet("get-course-by-id/{id}")]
        public async Task<IActionResult> GetCourseById(int id)
        {
            var course = await _courseService.GetCourseById(id);
            if (course == null)
            {
                return NotFound("course not found");
            }
            return Ok(course);
        }

        [Authorize(Roles = "Student")]
        [HttpPost("create-course")]
        public async Task<IActionResult> CreateCourse([FromBody] CourseRequestDto courseRequest)
        {
            try
            {
                var userId = _userService.GetMyUserId(User);
                var course = await _courseService.CreateCourseAsync(courseRequest, userId);
                return Ok(course);
            }
            catch (System.Exception)
            {
                return BadRequest("Failed to create course");
            }
        }

        [Authorize(Roles = "Student")]
        [HttpPut("update-course/{id}")]
        public async Task<IActionResult> UpdateCourse(int id, [FromBody] UpdateCourseDto dto)
        {
            if (dto == null)
            {
                return BadRequest("Update data is null.");
            }

            var result = await _courseService.UpdateCourseAsync(id, dto);

            if (result == null)
            {
                return NotFound(result);
            }

            return Ok(result);
        }

        [Authorize(Roles = "Student")]
        [HttpDelete("delete-course/{id}")]
        public async Task<IActionResult> DeleteCourse(int id)
        {
            var result = await _courseService.DeleteCourseById(id);

            if (!result.Success)
            {
                return NotFound(result);
            }

            // Có thể trả về Ok(result) hoặc NoContent() cho một yêu cầu DELETE thành công.
            return Ok(result);
        }
        [Authorize(Roles = "Student")]
        [HttpGet("{courseId}/events")]
        public async Task<IActionResult> GetParentEventsByCourse(int courseId)
        {
            var result = await _courseService.GetListParentEventByCourseId(courseId);
            return Ok(result);
        }

        [Authorize(Roles = "Student")]
        [HttpGet("{courseId}/topics")]
        public async Task<IActionResult> GetTopicByCourseId(int courseId)
        {
            try
            {
                var topics = await _topicService.GetTopicByCourseId(courseId);
                return Ok(topics);
            }
            catch (System.Exception)
            {
                return BadRequest("Failed to get topics");
            }
        }
    }
}
