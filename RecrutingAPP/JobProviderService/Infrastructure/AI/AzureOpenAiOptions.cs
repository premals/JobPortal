namespace JobProviderService.Infrastructure.AI
{
    public class AzureOpenAiOptions
    {
        public string Endpoint { get; set; } = null!;
        public string ApiKey { get; set; } = null!;
        public string Deployment { get; set; } = null!;
        public string ApiVersion { get; set; } = "2025-04-01-preview";
        public string AuthMode { get; set; } = "DefaultCredential";
        public string TokenScope { get; set; } = "https://cognitiveservices.azure.com/.default";
        public string? TenantId { get; set; }
        public string? ClientId { get; set; }
        public string? ClientSecret { get; set; }
        public string? ManagedIdentityClientId { get; set; }
    }
}
