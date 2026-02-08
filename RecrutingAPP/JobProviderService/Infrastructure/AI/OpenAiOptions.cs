namespace JobProviderService.Infrastructure.AI
{
    public class OpenAiOptions
    {
        public string ApiKey { get; set; } = string.Empty;
        public string BaseUrl { get; set; } = "https://api.openai.com/v1";
        public string Model { get; set; } = "gpt-4o-mini";
        public string? Organization { get; set; }
        public string? Project { get; set; }
    }
}
