using Synapse_API.Models.Enums;

namespace Synapse_API.Models.Dto.AuthDTOs
{
    public class RegisterResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string Token { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
    }
}
