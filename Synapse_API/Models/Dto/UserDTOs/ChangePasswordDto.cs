using System.ComponentModel.DataAnnotations;

namespace Synapse_API.Models.Dto.UserDTOs
{
    public class ChangePasswordDto
    {
        public string CurrentPassword { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 6)]
        public string NewPassword { get; set; }

        [Required]
        [Compare("NewPassword")]
        public string ConfirmNewPassword { get; set; }
    }
}
