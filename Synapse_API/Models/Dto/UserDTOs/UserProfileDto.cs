using System.ComponentModel.DataAnnotations;

namespace Synapse_API.Models.Dto.UserDTOs

{
    public class UserProfileDto
    {
        public int UserID { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }

        // Hồ sơ chi tiết
        public string? Major { get; set; }
        public string? Interests { get; set; }
        public string? Avatar { get; set; }
        public string? Address { get; set; }
        public int? DailyStudyHours { get; set; } // Số giờ có thể học mỗi ngày
        public string? PreferredStudyTime { get; set; } // "Morning", "Afternoon", "Evening"

        public string? PhoneNumber { get; set; }
    }
}
