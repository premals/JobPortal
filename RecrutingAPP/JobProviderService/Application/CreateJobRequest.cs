using System.ComponentModel.DataAnnotations;

namespace JobProviderService.Application
{
    public class CreateJobRequest
    {
        [Required(ErrorMessage = "Job title is required")]
        [StringLength(100, MinimumLength = 3)]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Job description is required")]
        [StringLength(5000, MinimumLength = 50)]
        public string Description { get; set; } = string.Empty;

        [Required]
        [RegularExpression("FullTime|PartTime|Contract|Internship",
            ErrorMessage = "Invalid employment type")]
        public string EmploymentType { get; set; } = string.Empty;

        [Required]
        [RegularExpression("Remote|Hybrid|Onsite",
            ErrorMessage = "Invalid work mode")]
        public string WorkMode { get; set; } = string.Empty;

        // --------------------
        // Experience
        // --------------------
        [Range(0, 50)]
        public int MinExperience { get; set; }

        [Range(0, 50)]
        public int MaxExperience { get; set; }

        // --------------------
        // Location
        // --------------------
        [Required]
        [StringLength(100)]
        public string City { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string State { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Country { get; set; } = string.Empty;

        // --------------------
        // Salary
        // --------------------
        [Range(0, double.MaxValue, ErrorMessage = "Minimum salary must be positive")]
        public decimal MinSalary { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Maximum salary must be positive")]
        public decimal MaxSalary { get; set; }

        [Required]
        [RegularExpression("INR|USD|CAD|EUR|GBP",
            ErrorMessage = "Unsupported currency")]
        public string Currency { get; set; } = "INR";

        [Required]
        [RegularExpression("Monthly|Yearly",
            ErrorMessage = "Salary frequency must be Monthly or Yearly")]
        public string SalaryFrequency { get; set; } = "Yearly";

        // --------------------
        // Skills & Education
        // --------------------
        [MinLength(1, ErrorMessage = "At least one skill is required")]
        public List<string> KeySkills { get; set; } = new();

        [Required]
        public string Education { get; set; } = string.Empty;

        [Required]
        public string Industry { get; set; } = string.Empty;

        // --------------------
        // Job Meta
        // --------------------
        [Range(1, 1000)]
        public int Openings { get; set; }

        public DateTime? ExpiryDate { get; set; }
    }
}
