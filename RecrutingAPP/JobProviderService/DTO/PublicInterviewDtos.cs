using JobProviderService.Domain;

namespace JobProviderService.DTO
{
    public class PublicInterviewInviteResponse
    {
        public string InviteId { get; set; } = string.Empty;
        public string JobTitle { get; set; } = string.Empty;
        public string CandidateName { get; set; } = string.Empty;
        public string CandidateEmail { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Difficulty { get; set; } = string.Empty;
        public int QuestionsCount { get; set; }
        public List<InterviewTimeSlot> ProposedSlots { get; set; } = new();
        public DateTime? SelectedSlot { get; set; }
        public DateTime? TokenExpiresAt { get; set; }
        public string? SessionId { get; set; }
    }

    public class PublicInterviewSessionResponse
    {
        public string SessionId { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime? ScheduledStart { get; set; }
        public DateTime? ScheduledEnd { get; set; }
        public List<string> Questions { get; set; } = new();
        public string? AvatarProvider { get; set; }
    }
}
