namespace JobProviderService.Infrastructure.Options
{
    public class BlobStorageOptions
    {
        public string? ConnectionString { get; set; }
        public string? AccountUrl { get; set; }
        public string ContainerName { get; set; } = "interview-recordings";
        public string? PublicBaseUrl { get; set; }
        public string PublicAccess { get; set; } = "blob";
    }
}
