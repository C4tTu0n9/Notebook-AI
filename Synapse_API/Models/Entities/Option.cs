using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Synapse_API.Models.Entities
{
    public class Option
    {
        [Key]
        public int OptionID { get; set; }

        [ForeignKey("Question")]
        public int QuestionID { get; set; }

        [Required]
        [StringLength(10)]
        public string OptionKey { get; set; } // Nội dung của lựa chọn (ví dụ: "A")

        [Required]
        [StringLength(255)]
        public string OptionText { get; set; } // Nội dung của lựa chọn (ví dụ: "Lựa chọn 1")

        public bool IsCorrect { get; set; }

        public Question Question { get; set; }

    }
}
