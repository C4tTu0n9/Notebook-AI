namespace Synapse_API.Models.Dto.AuthDTOs
{
    public class VerifyTokenRequest
    {
        public string Email {  get; set; }
        public string Token { get; set; }
    }
}
