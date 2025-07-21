using System.ComponentModel.DataAnnotations;

namespace Synapse_API.Models.Entities
{
    public class UserStatistic
    {
        [Key]
        public int StatID { get; set; }

        public int? TotalUsers { get; set; }

        public int? ActiveUsers { get; set; }

        public int? PremiumUsers { get; set; }

        public DateTime DateRecorded { get; set; } = DateTime.Today;
    }
}
