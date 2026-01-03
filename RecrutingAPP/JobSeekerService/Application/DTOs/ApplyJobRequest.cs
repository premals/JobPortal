using System.ComponentModel.DataAnnotations;

namespace JobSeekerService.Application.DTOs
{
    public class ApplyJobRequest
    {
        [Required(ErrorMessage = "JobId is required")]
        public string JobId { get; set; } = null!;

        [Required(ErrorMessage = "JobProviderId is required")]
        public string JobProviderId { get; set; } = null!;

        // ============================
        // Resume Information
        // ============================

        [Required(ErrorMessage = "Resume URL is required")]
        [Url(ErrorMessage = "ResumeUrl must be a valid URL")]
        public string ResumeUrl { get; set; } = null!;

        // ============================
        // Optional Fields
        // ============================

        [StringLength(1000, ErrorMessage = "Cover letter cannot exceed 1000 characters")]
        public string? CoverLetter { get; set; }
    }
}
