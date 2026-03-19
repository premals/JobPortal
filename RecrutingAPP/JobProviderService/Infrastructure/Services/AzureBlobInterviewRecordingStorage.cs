using Azure;
using Azure.Identity;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using JobProviderService.Application.Interfaces;
using JobProviderService.Infrastructure.Options;
using Microsoft.Extensions.Options;

namespace JobProviderService.Infrastructure.Services
{
    public class AzureBlobInterviewRecordingStorage : IInterviewRecordingStorage
    {
        private readonly BlobContainerClient _container;
        private readonly BlobStorageOptions _options;

        public AzureBlobInterviewRecordingStorage(IOptions<BlobStorageOptions> options)
        {
            _options = options.Value;
            var serviceClient = CreateServiceClient(_options);
            _container = serviceClient.GetBlobContainerClient(_options.ContainerName);
            CreateContainerIfMissing();
        }

        public async Task<string> UploadAsync(
            string blobPath,
            Stream content,
            string contentType,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(blobPath))
                throw new ArgumentException("blobPath is required", nameof(blobPath));

            var blob = _container.GetBlobClient(blobPath);
            var headers = new BlobHttpHeaders
            {
                ContentType = string.IsNullOrWhiteSpace(contentType) ? "application/octet-stream" : contentType
            };

            await blob.UploadAsync(content, headers, cancellationToken: cancellationToken);

            if (!string.IsNullOrWhiteSpace(_options.PublicBaseUrl))
            {
                var baseUrl = _options.PublicBaseUrl.TrimEnd('/');
                return $"{baseUrl}/{_options.ContainerName}/{blobPath}";
            }

            return blob.Uri.ToString();
        }

        private static BlobServiceClient CreateServiceClient(BlobStorageOptions options)
        {
            if (!string.IsNullOrWhiteSpace(options.AccountUrl))
            {
                var serviceUri = new Uri(options.AccountUrl);
                var credential = new DefaultAzureCredential();
                return new BlobServiceClient(serviceUri, credential);
            }

            if (!string.IsNullOrWhiteSpace(options.ConnectionString))
            {
                return new BlobServiceClient(options.ConnectionString);
            }

            throw new InvalidOperationException(
                "BlobStorage configuration requires ConnectionString or AccountUrl.");
        }

        private void CreateContainerIfMissing()
        {
            var publicAccess = ParsePublicAccess(_options.PublicAccess);
            try
            {
                _container.CreateIfNotExists(publicAccess);
            }
            catch (RequestFailedException ex)
                when (string.Equals(ex.ErrorCode, "PublicAccessNotPermitted", StringComparison.OrdinalIgnoreCase))
            {
                _container.CreateIfNotExists(PublicAccessType.None);
            }
        }

        private static PublicAccessType ParsePublicAccess(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return PublicAccessType.Blob;

            return value.Trim().ToLowerInvariant() switch
            {
                "container" => PublicAccessType.Blob,
                "none" => PublicAccessType.None,
                _ => PublicAccessType.Blob
            };
        }
    }
}
