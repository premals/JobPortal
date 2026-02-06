namespace JobProviderService.DTO
{
    public class InterviewInviteRequest
    {
        public string Difficulty { get; set; } = "Medium";
        public List<DateTime> ProposedSlots { get; set; } = new();
        public int QuestionsCount { get; set; } = 5;
    }

    public class InterviewInviteAcceptRequest
    {
        public DateTime SelectedSlot { get; set; }
    }
}
