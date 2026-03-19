using System.ComponentModel.DataAnnotations;

namespace JobSeekerService.Application.DTOs
{
    public class ApplyJobRequest
    {
        [Required(ErrorMessage = "JobId is required")]
        public string JobId { get; set; } = null!;

        // Optional from client; server resolves from job snapshot when omitted.
        public string? JobProviderId { get; set; }

        // ============================
        // Resume Information
        // ============================

        [Url(ErrorMessage = "ResumeUrl must be a valid URL")]
        public string? ResumeUrl { get; set; }

        // ============================
        // Optional Fields
        // ============================

        [StringLength(1000, ErrorMessage = "Cover letter cannot exceed 1000 characters")]
        public string? CoverLetter { get; set; }
    }
}
