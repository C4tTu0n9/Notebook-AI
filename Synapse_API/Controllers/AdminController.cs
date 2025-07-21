using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Synapse_API.Services.AmazonServices;
using Synapse_API.Services.CourseServices;
using Synapse_API.Services.DocumentServices.Interfaces;
using Synapse_API.Services;
using Synapse_API.Models.Entities;
using Synapse_API.Models.Dto.UserDTOs;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace Synapse_API.Controllers
{
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly UserService _userService;

        public AdminController( UserService userService)
        {
            _userService = userService;
        }

        [HttpGet("/api/admin/getAllUsers")]
        public async Task<ActionResult<IEnumerable<UserDto>>> getAllUsers()
        {
            var users = await _userService.GetAllUsersAsync();
            return Ok(users);
        }

        [HttpGet("/api/admin/getUser/{id}")]
        public async Task<ActionResult<UserDto>> GetUserByID(int id)
        {
            var userDto = await _userService.GetUserDtoAsync(id);
            if (userDto == null) return NotFound();
            return Ok(userDto);
        }

        [HttpPost("/api/admin/addUser")]
        public async Task<ActionResult<UserRequest>> AddUserAsync([FromBody] UserRequest userRequest)
        {
            try
            {
                var result = await _userService.AddUserAsync(userRequest);
                return Ok();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message }); 
            }
            catch (Exception)
            {
                return StatusCode(500, "An error occurred while adding the user.");
            }
        }

        [HttpPut("/api/admin/updateUser")]
        public async Task<ActionResult<UserRequest>> UpdateUserAsync([FromBody] UserRequest userRequest)
        {
            try
            {
                var user = await _userService.UpdateUserAsync(userRequest);
                if (user == null) return NotFound(); 
                return Ok();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(500, "An error occurred while adding the user.");
            }

        }   

         
        [HttpDelete("/api/admin/deleteUser/{userId}")]
        public async Task<ActionResult<UserRequest>> DeleteUserAsync(int userId)
        {
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            try
            {
                var deletedUser = await _userService.DeleteUserAsync(userId, currentUserId);
                if (deletedUser == null)
                {
                    return NotFound();
                }
            }
            catch (InvalidOperationException ex) {
                return BadRequest("Bạn không thể xóa chính tài khoản của mình.");
            }
            return Ok();
        }

    }
    public class UserRequest
    {
        public int UserID { get; set; }
        [Required(ErrorMessage = "Email là bắt buộc")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; }
        [Required]
        public string FullName { get; set; }
        public string? Password { get; set; }
        [Required(ErrorMessage = "Role is required.")]
        public string Role { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

    }
}
