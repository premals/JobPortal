using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace AdminService.Domain.Entities
{
    public class AdminJobProjection
    {
        [BsonId]
        public string JobId { get; set; } = string.Empty;

        public string JobProviderId { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string EmploymentType { get; set; } = string.Empty;
        public string WorkMode { get; set; } = string.Empty;
        public int MinExperience { get; set; }
        public int MaxExperience { get; set; }
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public decimal MinSalary { get; set; }
        public decimal MaxSalary { get; set; }
        public string Currency { get; set; } = "INR";
        public string SalaryFrequency { get; set; } = "Yearly";
        public List<string> KeySkills { get; set; } = new();
        public string Education { get; set; } = string.Empty;
        public string Industry { get; set; } = string.Empty;
        public int Openings { get; set; }
        public DateTime PostedAt { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public string Status { get; set; } = "Active";
        public DateTime? ClosedAt { get; set; }
    }
}
