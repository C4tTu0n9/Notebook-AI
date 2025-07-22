using Microsoft.AspNetCore.Mvc;
using Synapse_API.Services.EventServices;
using Synapse_API.Models.Dto.EventDTOs;
using Microsoft.AspNetCore.Authorization;
using Synapse_API.Services;
using Synapse_API.Models.Enums;

namespace Synapse_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EventController : ControllerBase
    {
        private readonly EventService _eventService;
        private readonly UserService _userService;

        public EventController(EventService eventService, UserService userService)
        {
            _eventService = eventService;
            _userService = userService;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<EventDto>> GetEventById(int id)
        {
            var e = await _eventService.GetEventById(id);
            if (e == null)
            {
                return NotFound();
            }
            return Ok(e);
        }
        [HttpGet("EventAndSubEvents/{id}")]
        public async Task<ActionResult<EventDto>> GetEventAndItsSubEventById(int id)
        {
            var e = await _eventService.GetEventAndItsSubEventById(id);
            if (e == null)
            {
                return NotFound();
            }
            return Ok(e);
        }
        [Authorize(Roles = "Student")]
        [HttpGet("MyEvents")]
        public async Task<ActionResult<IEnumerable<EventDto>>> GetMyEvents()
        {
            int userId = _userService.GetMyUserId(User);
            var events = await _eventService.GetEventsByStudentId(userId);

            if (!events.Any())
            {
                return NotFound($"No events found for student with ID {userId}.");
            }
            return Ok(events);
        }
        [Authorize(Roles = "Student")]
        [HttpGet("MyParentEvents")]
        public async Task<ActionResult<IEnumerable<EventDto>>> GetMyParentEvents()
        {
            int userId = _userService.GetMyUserId(User);
            var events = await _eventService.GetParentEventsByStudentId(userId);

            if (!events.Any())
            {
                return NotFound($"No parent events found for student with ID {userId}.");
            }
            return Ok(events);
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<EventDto>>> GetAllEvents()
        {
            var events = await _eventService.GetAllEvents();
            return Ok(events);
        }

        [Authorize(Roles = "Student")]
        [HttpPost]
        public async Task<ActionResult<EventDto>> CreateEvent([FromBody] CreateEventDto ce)
        {
            ce.UserID = _userService.GetMyUserId(User);
            var e = await _eventService.CreateEvent(ce);
            return CreatedAtAction(nameof(GetEventById), new { id = e.EventID }, e);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<EventDto>> UpdateEvent(int id, [FromBody] UpdateEventDto ue)
        {
            ue.EventID = id;
            var originalEvent = await _eventService.GetEventById(id);
            if (originalEvent == null)
                return NotFound();

            bool isTimeChanged = originalEvent.StartTime != ue.StartTime || originalEvent.EndTime != ue.EndTime;
            bool isExamType = ue.EventType == EventType.Exam.ToString();

            if (isTimeChanged || !isExamType)
            {
                await _eventService.DeleteChileEventsByParentEventId(id);
            }

            var updatedEvent = await _eventService.UpdateEvent(ue);
            return Ok(updatedEvent);
        }
        [HttpPut("{id}/UpdateIsCompleted")]
        public async Task<IActionResult> UpdateIsCompleted(int id, [FromBody] UpdateIsCompletedDto dto)
        {
            var e = await _eventService.GetEventById(id);
            if (e == null)
                return NotFound();

            await _eventService.UpdateChildEventIsCompleted(id, dto.IsCompleted);
            return Ok(new { success = true });
        }


        [HttpDelete("{id}")]
        public async Task<ActionResult<EventDto>> DeleteEvent(int id)
        {
            var e = await _eventService.DeleteEvent(id);
            return Ok(e);
        }

        // Tạo lịch trình ôn thi tự động
        [Authorize(Roles = "Student")]
        [HttpPost("generate-study-plan")]
        public async Task<ActionResult<List<StudySessionDto>>> GenerateStudyPlan([FromBody] GenerateStudyPlanDto generateDto)
        {
            try
            {
                int userId = _userService.GetMyUserId(User);
                generateDto.UserID = userId;
                var studySessions = await _eventService.GenerateStudyPlan(generateDto);
                return Ok(studySessions);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

    }
}
