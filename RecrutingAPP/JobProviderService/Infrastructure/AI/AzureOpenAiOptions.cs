namespace JobProviderService.Infrastructure.AI
{
    public class AzureOpenAiOptions
    {
        public string Endpoint { get; set; } = string.Empty;
        public string Deployment { get; set; } = string.Empty;
        public string ApiVersion { get; set; } = string.Empty;
        public string TokenScope { get; set; } = "https://cognitiveservices.azure.com/.default";
    }
}
