namespace JobProviderService.DTO
{
    public class JobApplicationSummaryResponse
    {
        public long TotalApplicants { get; set; }
        public long AppliedCount { get; set; }
        public long ShortlistedCount { get; set; }
        public long RejectedCount { get; set; }
        public long HiredCount { get; set; }
        public List<JobApplicationSummaryItem> Recent { get; set; } = new();
    }

    public class JobApplicationSummaryItem
    {
        public string JobId { get; set; } = null!;
        public string JobTitle { get; set; } = null!;
        public string JobSeekerId { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Phone { get; set; } = null!;
        public string ResumeUrl { get; set; } = null!;
        public string Status { get; set; } = null!;
        public DateTime AppliedAt { get; set; }
    }
}
