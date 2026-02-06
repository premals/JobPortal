using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace JobProviderService.Domain
{
    public class InterviewInvite
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = null!;

        public string JobId { get; set; } = null!;
        public string JobTitle { get; set; } = null!;
        public string JobProviderId { get; set; } = null!;
        public string JobSeekerId { get; set; } = null!;

        public string CandidateName { get; set; } = null!;
        public string CandidateEmail { get; set; } = null!;

        public string Difficulty { get; set; } = "Medium";
        public string Status { get; set; } = "Pending";
        public int QuestionsCount { get; set; } = 5;

        public List<InterviewTimeSlot> ProposedSlots { get; set; } = new();
        public DateTime? SelectedSlot { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }

    public class InterviewTimeSlot
    {
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
    }
}
