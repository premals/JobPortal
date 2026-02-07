using MongoDB.Bson.Serialization.Attributes;

namespace AdminService.Domain.Entities
{
    public class AdminApplicationProjection
    {
        [BsonId]
        public string ApplicationId { get; set; } = string.Empty;

        public string JobId { get; set; } = string.Empty;
        public string JobProviderId { get; set; } = string.Empty;
        public string JobSeekerId { get; set; } = string.Empty;

        public string CandidateName { get; set; } = string.Empty;
        public string CandidateEmail { get; set; } = string.Empty;
        public string? CandidatePhone { get; set; }
        public string? ResumeUrl { get; set; }

        public string Status { get; set; } = "Applied";
        public DateTime AppliedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
