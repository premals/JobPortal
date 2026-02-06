namespace JobProviderService.DTO
{
    public class JobProviderSettingsDto
    {
        public InterviewSettingsDto Interview { get; set; } = new();
        public AiSettingsDto Ai { get; set; } = new();
        public EmailTemplateSettingsDto Email { get; set; } = new();
    }

    public class InterviewSettingsDto
    {
        public List<string> DifficultyLevels { get; set; } = new();
        public string DefaultDifficulty { get; set; } = "Medium";
        public int QuestionsCount { get; set; } = 5;
        public int SlotDurationMinutes { get; set; } = 45;
        public int SlotCount { get; set; } = 3;
    }

    public class AiSettingsDto
    {
        public bool EnableShortlistSuggestions { get; set; } = true;
        public bool EnableInterviewAi { get; set; } = true;
        public string? Endpoint { get; set; }
        public string? Deployment { get; set; }
        public string? ApiVersion { get; set; } = "2025-04-01-preview";
        public bool EnableAvatar { get; set; } = false;
        public string? AvatarProvider { get; set; }
    }

    public class EmailTemplateSettingsDto
    {
        public string InviteSubject { get; set; } = string.Empty;
        public string InviteBody { get; set; } = string.Empty;
    }
}
