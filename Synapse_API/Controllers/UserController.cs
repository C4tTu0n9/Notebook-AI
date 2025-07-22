using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using Synapse_API.Data;
using Synapse_API.Models.Dto.UserDTOs;
using Synapse_API.Models.Entities;
using Synapse_API.Services;
using System.Security.Claims;

namespace Synapse_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly UserService _userService;

        public UserController(UserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetUsers()
        {
            var users = await _userService.GetAllUserDtosAsync();
            return Ok(users);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<UserDto>> GetUser(int id)
        {
            var userDto = await _userService.GetUserDtoAsync(id);
            if (userDto == null) return NotFound();
            return Ok(userDto);
        }

        [HttpGet("get-profile")]
        public async Task<IActionResult> GetProfile()
        {
            
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr))
                return Unauthorized("User ID missing from token.");

            int userId = int.Parse(userIdStr);

            var profileDto = await _userService.GetUserProfileAsync(userId);

            if (profileDto == null)
                return NotFound("User not found");

            return Ok(profileDto);
        }

        [HttpPut("update-profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateUserProfileDto dto)
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr))
                return Unauthorized("User ID missing from token.");

            int userId = int.Parse(userIdStr);

            var success = await _userService.UpdateUserProfileAsync(userId, dto);
            if (!success)
                return NotFound("User or profile not found.");

            return Ok("Profile updated successfully.");
        }


        [HttpPut("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr))
                return Unauthorized("User ID missing from token.");

            int userId = int.Parse(userIdStr);

            var success = await _userService.ChangePasswordAsync(userId, dto);
            if(success == true)
            {
                return Ok("Changepassword successfully.");

            }
            return BadRequest("Changepassword NOT successfully.");
        }


        [HttpGet("UserProfile")]
        public async Task<ActionResult<UserProfileDto>> GetUserProfile()
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr))
                return Unauthorized("User ID missing from token.");

            int userId = int.Parse(userIdStr);

            var userDto = await _userService.GetUserProfileAsync(userId);
            if (userDto == null)
                return NotFound("User not found");

            return Ok(userDto);
        }



    }
}