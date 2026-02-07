using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace AdminService.Domain.Entities
{
    public class AdminJobSeekerProjection
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = null!;

        public string UserId { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string? Headline { get; set; }
        public string? Summary { get; set; }
        public List<string> Skills { get; set; } = new();
        public int ExperienceYears { get; set; }
        public string Education { get; set; } = string.Empty;
        public string? Location { get; set; }
        public List<AdminWorkExperience> WorkHistory { get; set; } = new();
        public List<AdminEducationRecord> EducationHistory { get; set; } = new();
        public List<AdminProjectRecord> Projects { get; set; } = new();
        public List<AdminCertificationRecord> Certifications { get; set; } = new();
        public List<AdminLanguageRecord> Languages { get; set; } = new();
        public AdminResumeSettings ResumeSettings { get; set; } = new();

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastActiveAt { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class AdminWorkExperience
    {
        public string Company { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string StartDate { get; set; } = string.Empty;
        public string EndDate { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public List<string> Skills { get; set; } = new();
    }

    public class AdminEducationRecord
    {
        public string School { get; set; } = string.Empty;
        public string Degree { get; set; } = string.Empty;
        public string Field { get; set; } = string.Empty;
        public string GraduationYear { get; set; } = string.Empty;
    }

    public class AdminProjectRecord
    {
        public string Name { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? Link { get; set; }
    }

    public class AdminCertificationRecord
    {
        public string Name { get; set; } = string.Empty;
        public string Issuer { get; set; } = string.Empty;
        public string Year { get; set; } = string.Empty;
    }

    public class AdminLanguageRecord
    {
        public string Name { get; set; } = string.Empty;
        public string Proficiency { get; set; } = string.Empty;
    }

    public class AdminResumeSettings
    {
        public bool AtsFriendly { get; set; } = true;
        public string Template { get; set; } = "Clean";
    }
}
