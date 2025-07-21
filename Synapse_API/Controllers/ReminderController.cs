using Microsoft.AspNetCore.Mvc;
using Synapse_API.Services.EventServices;
using Synapse_API.Services;
using Microsoft.AspNetCore.Authorization;
using Synapse_API.Models.Entities;
using Synapse_API.Models.Dto.ReminderDTOs;

namespace Synapse_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReminderController : ControllerBase
    {
        private readonly EventReminderService _reminderService;
        private readonly EventService _eventService;
        private readonly UserService _userService;
        public ReminderController(EventReminderService reminderService, UserService userService, EventService eventService)
        {
            _reminderService = reminderService;
            _userService = userService;
            _eventService = eventService;
        }



        // Event Reminder Management Endpoints

        // Tạo reminder cho event (theo người dùng tự đặt)
        [Authorize(Roles = "Student")]
        [HttpPost("CreateFor/Event/{eventId}")]
        public async Task<ActionResult> CreateEventReminder(int eventId, [FromBody] CreateReminderRequest request)
        {
            try
            {
                // Kiểm tra event có thuộc về user hiện tại không
                var eventItem = await _eventService.GetEventById(eventId);
                if (eventItem == null)
                    return NotFound("Event không tồn tại");

                int userId = _userService.GetMyUserId(User);
                if (eventItem.UserID != userId)
                    return Forbid("Bạn không có quyền tạo reminder cho event này");

                bool success;
                if (request.ReminderTime.HasValue)  //giờ cụ thể
                {
                    success = await _reminderService.CreateReminderAsync(eventId, request.ReminderTime.Value);
                }
                else    //trước sự kiện bao nhiêu phút
                {
                    success = await _reminderService.CreateReminderAsync(eventId, request.MinutesBefore ?? 15);
                }

                if (success)
                {
                    return Ok(new { success = true, message = "Đã tạo reminder thành công" });
                }
                else
                {
                    return BadRequest(new { success = false, message = "Không thể tạo reminder" });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // Lấy tất cả reminders của một event
        [Authorize(Roles = "Student")]
        [HttpGet("OfEvent/{eventId}")]
        public async Task<ActionResult<IEnumerable<ReminderDto>>> GetEventReminders(int eventId)
        {
            try
            {
                // Kiểm tra quyền truy cập
                var eventItem = await _eventService.GetEventById(eventId);
                if (eventItem == null)
                    return NotFound("Event không tồn tại");

                int userId = _userService.GetMyUserId(User);
                if (eventItem.UserID != userId)
                    return Forbid("Bạn không có quyền xem reminders của event này");

                var reminders = await _reminderService.GetRemindersByEventIdAsync(eventId);
                return Ok(reminders);
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        // Lấy tất cả reminders của user hiện tại
        [Authorize(Roles = "Student")]
        [HttpGet("MyReminders")]
        public async Task<ActionResult<IEnumerable<ReminderDto>>> GetMyReminders(bool includeSent = false)
        {
            try
            {
                int userId = _userService.GetMyUserId(User);
                var reminders = await _reminderService.GetRemindersByUserIdAsync(userId, includeSent);
                return Ok(new { success = true, data = reminders });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        // Xóa một reminder
        [Authorize(Roles = "Student")]
        [HttpDelete("{reminderId}")]
        public async Task<ActionResult> DeleteEventReminder(int reminderId)
        {
            try
            {
                var success = await _reminderService.DeleteReminderAsync(reminderId);
                if (success)
                {
                    return Ok(new { success = true, message = "Đã xóa reminder thành công" });
                }
                else
                {
                    return NotFound(new { success = false, message = "Reminder không tồn tại" });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        // Cập nhật reminders cho một event
        [Authorize(Roles = "Student")]
        [HttpPut("UpdateFor/Event/{eventId}")]
        public async Task<ActionResult> UpdateEventReminders(int eventId, [FromBody] UpdateRemindersRequest request)
        {
            try
            {
                // Kiểm tra quyền truy cập
                var eventItem = await _eventService.GetEventById(eventId);
                if (eventItem == null)
                    return NotFound("Event không tồn tại");

                int userId = _userService.GetMyUserId(User);
                if (eventItem.UserID != userId)
                    return Forbid("Bạn không có quyền cập nhật reminders cho event này");

                var success = await _reminderService.UpdateRemindersAsync(eventId, request.ReminderMinutesBefore);
                if (success)
                {
                    return Ok(new { success = true, message = "Đã cập nhật reminders thành công" });
                }
                else
                {
                    return BadRequest(new { success = false, message = "Không thể cập nhật reminders" });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
    }
}
