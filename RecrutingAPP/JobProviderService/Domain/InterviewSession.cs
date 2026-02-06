using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace JobProviderService.Domain
{
    public class InterviewSession
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = null!;

        public string InviteId { get; set; } = null!;
        public string JobId { get; set; } = null!;
        public string JobProviderId { get; set; } = null!;
        public string JobSeekerId { get; set; } = null!;

        public string Difficulty { get; set; } = "Medium";
        public List<string> Skills { get; set; } = new();

        public string Status { get; set; } = "Scheduled"; // Scheduled, InProgress, Completed
        public int TotalQuestions { get; set; } = 5;
        public int CurrentQuestionIndex { get; set; } = 0;

        public List<string> Questions { get; set; } = new();
        public List<InterviewMessage> Transcript { get; set; } = new();

        public InterviewEvaluation? Evaluation { get; set; }

        // Video avatar hooks
        public string? AvatarProvider { get; set; }
        public string? AvatarSessionUrl { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }

    public class InterviewMessage
    {
        public string Role { get; set; } = null!; // system, assistant, user
        public string Content { get; set; } = null!;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }

    public class InterviewEvaluation
    {
        public int OverallScore { get; set; }
        public List<SkillScore> SkillScores { get; set; } = new();
        public string Summary { get; set; } = null!;
        public DateTime EvaluatedAt { get; set; } = DateTime.UtcNow;
    }

    public class SkillScore
    {
        public string Skill { get; set; } = null!;
        public int Score { get; set; }
        public string Feedback { get; set; } = null!;
    }
}
