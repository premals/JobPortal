namespace JobProviderService.Domain
{
    public class Job
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();

        // Identity
        public string JobProviderId { get; set; } = null!;

        // Basic Job Info
        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string EmploymentType { get; set; } = null!; // FullTime, PartTime, Contract
        public string WorkMode { get; set; } = null!;       // Remote, Hybrid, Onsite

        // Experience
        public int MinExperience { get; set; }
        public int MaxExperience { get; set; }

        // Location
        public string City { get; set; } = null!;
        public string State { get; set; } = null!;
        public string Country { get; set; } = null!;

        // Salary
        public decimal MinSalary { get; set; }
        public decimal MaxSalary { get; set; }
        public string Currency { get; set; } = "INR"; // INR, USD, CAD
        public string SalaryFrequency { get; set; } = "Yearly"; // Monthly, Yearly

        // Skills & Education
        public List<string> KeySkills { get; set; } = new();
        public string Education { get; set; } = null!;
        public string Industry { get; set; } = null!;

        // Job Meta
        public int Openings { get; set; }
        public DateTime PostedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ExpiryDate { get; set; }
        public string Status { get; set; } = "Active"; // Draft, Active, Closed
    }
}
