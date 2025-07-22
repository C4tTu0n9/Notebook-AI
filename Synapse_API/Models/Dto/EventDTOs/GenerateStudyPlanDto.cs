using System;

namespace Synapse_API.Models.Dto.EventDTOs
{
    public class GenerateStudyPlanDto
    {
        public int UserID { get; set; }
        public int CourseID { get; set; }
        public int ExamEventID { get; set; } // ID của event thi
        public DateTime ExamDate { get; set; }
        public int? DaysBeforeExam { get; set; } // Số ngày muốn ôn thi trước khi thi
    }
} 