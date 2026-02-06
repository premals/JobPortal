namespace JobProviderService.DTO
{
    public class AiShortlistSuggestionResponse
    {
        public string Recommendation { get; set; } = null!;
        public int Score { get; set; }
        public string Reasoning { get; set; } = null!;
        public List<string> Risks { get; set; } = new();
    }
}
