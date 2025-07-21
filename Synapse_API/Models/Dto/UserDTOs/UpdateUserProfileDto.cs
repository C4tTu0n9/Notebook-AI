using System.ComponentModel.DataAnnotations;

namespace Synapse_API.Models.Dto.UserDTOs
{
    public class UpdateUserProfileDto
    {
        [Required(ErrorMessage = "Name cannot be empty!")]
        [StringLength(100, ErrorMessage = "Name must be between 2 and 100 characters.", MinimumLength = 2)]
        [RegularExpression(@"^[a-zA-ZÀ-Ỹà-ỹ0-9\s]+$", ErrorMessage = "Name can only contain letters, numbers, and spaces.")]

        public string FullName { get; set; }
        public string? Major { get; set; }
        public string? Interests { get; set; }
        public string? Avatar { get; set; }
        public string? Address { get; set; }


        [StringLength(10, ErrorMessage = "Phone number cannot exceed 10 characters.")]
        [Required(ErrorMessage = "Phone number is required.")]
        [RegularExpression(@"^0\d{9}$", ErrorMessage = "Invalid phone number format!")]
        public string? PhoneNumber { get; set; }

        [Range(1, 24, ErrorMessage = "Daily Study Hours must be between 1 and 24.")]
        public int? DailyStudyHours { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập thời gian học ưa thích.")]
        [RegularExpression("^(Morning|Afternoon|Evening)$", ErrorMessage = "Giá trị phải là 'Morning', 'Afternoon' hoặc 'Evening'.")]
        public string? PreferredStudyTime { get; set; }

    }
}
