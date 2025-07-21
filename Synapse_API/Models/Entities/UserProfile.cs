using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Synapse_API.Models.Entities
{
    public class UserProfile
    {

        [Key]
        public int ProfileID { get; set; }

        [ForeignKey("User")]
        public int UserID { get; set; }

        [StringLength(100)]
        public string? Major { get; set; }

        [StringLength(255)]
        public string? Goals { get; set; }

        [StringLength(255)]
        public string? Interests { get; set; }

        [StringLength(255)]
        public string? CVFilePath { get; set; }

        [StringLength(255)]
        public string? TranscriptFilePath { get; set; }

        // Thêm thông tin về thời gian học tập mỗi ngày
        public int? DailyStudyHours { get; set; } // Số giờ có thể học mỗi ngày

        // Thời gian ưu tiên học (sáng/chiều/tối)
        [StringLength(50)]
        public string? PreferredStudyTime { get; set; } // "Morning", "Afternoon", "Evening"

        [StringLength(255)]
        public string? Avatar { get; set; }

        [StringLength(20)]
        public string? PhoneNumber { get; set; }

        [StringLength(255)]
        public string? Address { get; set; }

        public User User { get; set; }
    }
}