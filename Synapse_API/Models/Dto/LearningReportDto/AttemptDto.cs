namespace Synapse_API.Models.Dto.LearningReportDto
{
    public class AttemptDto
    {
        public int AttemptID { get; set; }
        public int QuizID { get; set; }
        public string QuizTitle { get; set; }
        public double Score { get; set; }
        public DateTime AttemptDate { get; set; }
        public string Feedback { get; set; }
    }
}
