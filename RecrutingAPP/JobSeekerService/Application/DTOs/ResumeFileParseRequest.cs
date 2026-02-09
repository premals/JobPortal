namespace JobSeekerService.Application.DTOs
{
    public class ResumeFileParseRequest
    {
        public byte[] Content { get; set; } = Array.Empty<byte>();
        public string FileName { get; set; } = string.Empty;
        public string? ContentType { get; set; }
    }
}
