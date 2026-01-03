namespace JobProviderService.DTO
{
    public class UpdateJobPatchRequest
    {
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? EmploymentType { get; set; }
        public string? WorkMode { get; set; }

        public int? MinExperience { get; set; }
        public int? MaxExperience { get; set; }

        public string? City { get; set; }
        public string? State { get; set; }
        public string? Country { get; set; }

        public decimal? MinSalary { get; set; }
        public decimal? MaxSalary { get; set; }
        public string? Currency { get; set; }
        public string? SalaryFrequency { get; set; }

        public List<string>? KeySkills { get; set; }
        public string? Education { get; set; }
        public string? Industry { get; set; }

        public int? Openings { get; set; }
        public DateTime? ExpiryDate { get; set; }
    }
}
