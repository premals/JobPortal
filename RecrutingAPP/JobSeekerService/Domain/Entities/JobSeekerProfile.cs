using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace JobSeekerService.Domain.Entities
{
    public class JobSeekerProfile
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = null!;

        public string UserId { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string? Phone { get; set; }
        public string? Headline { get; set; }
        public string? Summary { get; set; }
        public List<string> Skills { get; set; } = new();
        public int ExperienceYears { get; set; }
        public string Education { get; set; } = null!;
        public string? Location { get; set; }
        public List<WorkExperience> WorkHistory { get; set; } = new();
        public List<EducationRecord> EducationHistory { get; set; } = new();
        public List<ProjectRecord> Projects { get; set; } = new();
        public List<CertificationRecord> Certifications { get; set; } = new();
        public List<LanguageRecord> Languages { get; set; } = new();
        public ResumeSettings ResumeSettings { get; set; } = new();

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }

    public class WorkExperience
    {
        public string Company { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string StartDate { get; set; } = string.Empty;
        public string EndDate { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public List<string> Skills { get; set; } = new();
    }

    public class EducationRecord
    {
        public string School { get; set; } = string.Empty;
        public string Degree { get; set; } = string.Empty;
        public string Field { get; set; } = string.Empty;
        public string GraduationYear { get; set; } = string.Empty;
    }

    public class ProjectRecord
    {
        public string Name { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? Link { get; set; }
    }

    public class CertificationRecord
    {
        public string Name { get; set; } = string.Empty;
        public string Issuer { get; set; } = string.Empty;
        public string Year { get; set; } = string.Empty;
    }

    public class LanguageRecord
    {
        public string Name { get; set; } = string.Empty;
        public string Proficiency { get; set; } = string.Empty;
    }

    public class ResumeSettings
    {
        public bool AtsFriendly { get; set; } = true;
        public string Template { get; set; } = "Clean";
    }
}
