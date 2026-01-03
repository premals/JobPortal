namespace JobSeekerService.Application.DTOs
{
    public class ResumeAiRequest
    {
        public string FullName { get; set; } = null!;
        public List<string> Skills { get; set; } = new();
        public int ExperienceYears { get; set; }
        public string Education { get; set; } = null!;
        public string TargetRole { get; set; } = null!;
    }
}
