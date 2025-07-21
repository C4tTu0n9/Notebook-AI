using Synapse_API.Models.Dto.EventDTOs;

namespace Synapse_API.Models.Dto.UserDTOs
{
    public class UserDto
    {
        public int UserID { get; set; }
        public string Email { get; set; }
        public string FullName { get; set; }
        public string Role { get; set; }
        public bool IsActive { get; set; }
        public string Password { get; set; } = "";
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public List<EventDto> Events { get; set; } = new List<EventDto>();
        // ... add các collection khác nếu muốn
    }

}
