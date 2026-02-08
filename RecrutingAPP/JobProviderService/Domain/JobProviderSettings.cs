using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace JobProviderService.Domain
{
    public class JobProviderSettings
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = null!;

        public string JobProviderId { get; set; } = null!;
        public InterviewSettings Interview { get; set; } = new();
        public AiSettings Ai { get; set; } = new();
        public EmailTemplateSettings Email { get; set; } = new();
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }

    public class InterviewSettings
    {
        public List<string> DifficultyLevels { get; set; } = new() { "Easy", "Medium", "High" };
        public string DefaultDifficulty { get; set; } = "Medium";
        public int QuestionsCount { get; set; } = 5;
        public int SlotDurationMinutes { get; set; } = 45;
        public int SlotCount { get; set; } = 3;
    }

    public class AiSettings
    {
        public bool EnableShortlistSuggestions { get; set; } = true;
        public bool EnableInterviewAi { get; set; } = true;
        public string? Endpoint { get; set; }
        public string? Deployment { get; set; }
        public string? ApiVersion { get; set; }
        public bool EnableAvatar { get; set; } = false;
        public string? AvatarProvider { get; set; }
    }

    public class EmailTemplateSettings
    {
        public string InviteSubject { get; set; } = "Interview invitation for {JobTitle}";
        public string InviteBody { get; set; } =
            "Hello {CandidateName}, you have been invited to interview for {JobTitle}. " +
            "Please open your dashboard and choose a time slot.";
    }
}
