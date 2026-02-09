using JobSeekerService.Domain.Entities;

namespace JobSeekerService.Application.DTOs
{
    public class ResumeDraftRequest
    {
        public string? Template { get; set; }
        public bool AtsFriendly { get; set; } = true;
        public string FullName { get; set; } = string.Empty;
        public string? Headline { get; set; }
        public string Email { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string? Location { get; set; }
        public string? Summary { get; set; }
        public List<string> Skills { get; set; } = new();
        public List<WorkExperience> WorkHistory { get; set; } = new();
        public List<EducationRecord> EducationHistory { get; set; } = new();
        public List<ProjectRecord> Projects { get; set; } = new();
        public List<CertificationRecord> Certifications { get; set; } = new();
        public List<LanguageRecord> Languages { get; set; } = new();
        public string? AiGeneratedText { get; set; }
    }
}
