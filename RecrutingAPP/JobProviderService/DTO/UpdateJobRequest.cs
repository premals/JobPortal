using System.ComponentModel.DataAnnotations;

namespace JobProviderService.DTO
{
    public class UpdateJobRequest
    {
       [Required, StringLength(100, MinimumLength = 3)]
        public string Title { get; set; } = string.Empty;

        [Required, StringLength(5000, MinimumLength = 50)]
        public string Description { get; set; } = string.Empty;

        [Required]
        public string EmploymentType { get; set; } = string.Empty;

        [Required]
        public string WorkMode { get; set; } = string.Empty;

        [Range(0, 50)]
        public int MinExperience { get; set; }

        [Range(0, 50)]
        public int MaxExperience { get; set; }

        [Required]
        public string City { get; set; } = string.Empty;

        [Required]
        public string State { get; set; } = string.Empty;

        [Required]
        public string Country { get; set; } = string.Empty;

        [Range(0, double.MaxValue)]
        public decimal MinSalary { get; set; }

        [Range(0, double.MaxValue)]
        public decimal MaxSalary { get; set; }

        [Required]
        public string Currency { get; set; } = "INR";

        [Required]
        public string SalaryFrequency { get; set; } = "Yearly";

        [MinLength(1)]
        public List<string> KeySkills { get; set; } = new();

        [Required]
        public string Education { get; set; } = string.Empty;

        [Required]
        public string Industry { get; set; } = string.Empty;

        [Range(1, 1000)]
        public int Openings { get; set; }

        public DateTime? ExpiryDate { get; set; }
    }
}
