using JobProviderService.Domain;

namespace JobProviderService.DTO
{
    public class InterviewAnswerRequest
    {
        public string Answer { get; set; } = null!;
    }

    public class InterviewStartResponse
    {
        public string SessionId { get; set; } = null!;
        public string Question { get; set; } = null!;
        public int QuestionIndex { get; set; }
        public int TotalQuestions { get; set; }
    }

    public class InterviewAnswerResponse
    {
        public bool Completed { get; set; }
        public string? NextQuestion { get; set; }
        public int QuestionIndex { get; set; }
        public int TotalQuestions { get; set; }
        public InterviewEvaluation? Evaluation { get; set; }
    }

    public class InterviewReportResponse
    {
        public string SessionId { get; set; } = null!;
        public string JobId { get; set; } = null!;
        public string JobSeekerId { get; set; } = null!;
        public string Status { get; set; } = null!;
        public string Difficulty { get; set; } = null!;
        public List<string> Skills { get; set; } = new();
        public int TotalQuestions { get; set; }
        public DateTime UpdatedAt { get; set; }
        public InterviewEvaluation? Evaluation { get; set; }
    }
}
