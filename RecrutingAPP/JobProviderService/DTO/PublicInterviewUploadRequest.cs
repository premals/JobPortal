using Microsoft.AspNetCore.Http;

namespace JobProviderService.DTO
{
    public class PublicInterviewUploadRequest
    {
        public int QuestionIndex { get; set; }
        public string? AnswerText { get; set; }
        public double DurationSeconds { get; set; }
        public IFormFile? Video { get; set; }
    }
}
