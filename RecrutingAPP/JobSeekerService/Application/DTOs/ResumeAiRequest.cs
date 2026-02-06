namespace JobSeekerService.Application.DTOs
{
    public class ResumeAiRequest
    {
        public string FullName { get; set; } = null!;
        public List<string> Skills { get; set; } = new();
        public int ExperienceYears { get; set; }
        public string Education { get; set; } = null!;
        public string TargetRole { get; set; } = null!;
        public string? Summary { get; set; }
        public List<Domain.Entities.WorkExperience> WorkHistory { get; set; } = new();
        public List<Domain.Entities.ProjectRecord> Projects { get; set; } = new();
        public List<Domain.Entities.CertificationRecord> Certifications { get; set; } = new();
        public bool AtsFriendly { get; set; } = true;
        public string Template { get; set; } = "Clean";
    }
}
