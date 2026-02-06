using JobSeekerService.Application.DTOs;
using JobSeekerService.Application.Interfaces;
using System.Linq;

namespace JobSeekerService.Infrastructure.AI
{
    public class ResumeAiService : IResumeAiService
    {
        public Task<string> GenerateResumeAsync(ResumeAiRequest req)
        {
            var experience = req.WorkHistory.Count == 0
                ? "Experience details not provided."
                : string.Join("\n", req.WorkHistory.Select(exp =>
                    $"- {exp.Role} at {exp.Company} ({exp.StartDate} - {exp.EndDate}): {exp.Description}"));

            var projects = req.Projects.Count == 0
                ? "Projects not listed."
                : string.Join("\n", req.Projects.Select(p =>
                    $"- {p.Name} ({p.Role}): {p.Description} {p.Link}".Trim()));

            var certifications = req.Certifications.Count == 0
                ? "Certifications not listed."
                : string.Join(", ", req.Certifications.Select(c => $"{c.Name} ({c.Issuer}, {c.Year})"));

            var summary = string.IsNullOrWhiteSpace(req.Summary)
                ? $"Results-driven professional with {req.ExperienceYears}+ years of experience in {req.TargetRole}."
                : req.Summary;

            var atsNote = req.AtsFriendly ? "ATS-optimized summary and bullets." : "Design-forward summary.";

            var resume = $"""
        {req.FullName}
        Target Role: {req.TargetRole}

        Summary:
        {summary}

        Core Skills:
        {string.Join(", ", req.Skills)}

        Experience:
        {experience}

        Projects:
        {projects}

        Certifications:
        {certifications}

        Education:
        {req.Education}

        Template: {req.Template} | {atsNote}
        """;

            return Task.FromResult(resume);
        }
    }
}
