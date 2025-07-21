namespace Synapse_API.Models.Dto.AuthDTOs
{
    public class LoginResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string Token { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public string FullName {  get; set; }
    }
}
