namespace JobProviderService.Infrastructure.Options
{
    public class AppUrlOptions
    {
        public string? FrontendBaseUrl { get; set; }
        public string PublicInterviewPath { get; set; } = "/public-interview";
    }
}
