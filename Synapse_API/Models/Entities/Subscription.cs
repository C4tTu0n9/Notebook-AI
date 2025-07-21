using Synapse_API.Models.Enums;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Synapse_API.Models.Entities
{
    public class Subscription
    {
        [Key]
        public int SubscriptionID { get; set; }

        [ForeignKey("User")]
        public int UserID { get; set; }

        [StringLength(50)]
        public PlanType PlanType { get; set; } = PlanType.Free;

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public bool IsActive { get; set; } = true;

        public User User { get; set; }
    }
}