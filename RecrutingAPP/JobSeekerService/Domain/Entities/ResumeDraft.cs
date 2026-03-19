using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace JobSeekerService.Domain.Entities
{
    public class ResumeDraft
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = null!;

        public string UserId { get; set; } = string.Empty;
        public string Template { get; set; } = "modern";
        public bool AtsFriendly { get; set; } = true;
        public string FullName { get; set; } = string.Empty;
        public string? Headline { get; set; }
        public string Email { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string? Location { get; set; }
        public string? Summary { get; set; }
        public List<string> Skills { get; set; } = new();
        public int? ExperienceYears { get; set; }
        public string? Education { get; set; }
        public List<WorkExperience> WorkHistory { get; set; } = new();
        public List<EducationRecord> EducationHistory { get; set; } = new();
        public List<ProjectRecord> Projects { get; set; } = new();
        public List<CertificationRecord> Certifications { get; set; } = new();
        public List<LanguageRecord> Languages { get; set; } = new();
        public string? AiGeneratedText { get; set; }
        public DateTime? LastParsedAt { get; set; }
        public DateTime? LastGeneratedAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
