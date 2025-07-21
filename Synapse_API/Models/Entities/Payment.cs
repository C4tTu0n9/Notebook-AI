using Synapse_API.Models.Enums;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Synapse_API.Models.Entities
{
    public class Payment
    {
        [Key]
        public int PaymentID { get; set; }

        [ForeignKey("User")]
        public int UserID { get; set; }

        [Required]
        public decimal Amount { get; set; }

        public DateTime PaymentDate { get; set; } = DateTime.Now;

        [StringLength(50)]
        public PaymentMethodEnum PaymentMethod { get; set; }
        [StringLength(50)]
        public PaymentStatus Status { get; set; } = PaymentStatus.Pending;

        public User User { get; set; }
    }
}