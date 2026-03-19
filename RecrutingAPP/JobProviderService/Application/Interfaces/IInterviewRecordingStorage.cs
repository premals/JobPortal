namespace JobProviderService.Application.Interfaces
{
    public interface IInterviewRecordingStorage
    {
        Task<string> UploadAsync(
            string blobPath,
            Stream content,
            string contentType,
            CancellationToken cancellationToken = default);
    }
}
