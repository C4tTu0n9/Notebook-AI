using System.ComponentModel.DataAnnotations;

namespace Synapse_API.Models.Entities
{
    public class RevenueReport
    {
        [Key]
        public int ReportID { get; set; }

        public DateTime ReportDate { get; set; }

        public decimal? TotalRevenue { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
