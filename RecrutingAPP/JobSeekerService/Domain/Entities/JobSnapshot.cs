using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace JobSeekerService.Domain.Entities
{
    public class JobSnapshot
    {
        // ============================
        // Mongo Identity
        // ============================
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = null!;

        // ============================
        // Job Identity
        // ============================
        [BsonElement("jobId")]
        public string JobId { get; set; } = null!;

        [BsonElement("jobProviderId")]
        public string JobProviderId { get; set; } = null!;

        // ============================
        // Basic Job Info
        // ============================
        [BsonElement("title")]
        public string Title { get; set; } = null!;

        [BsonElement("description")]
        public string Description { get; set; } = null!;

        [BsonElement("employmentType")]
        public string EmploymentType { get; set; } = null!;

        [BsonElement("workMode")]
        public string WorkMode { get; set; } = null!;

        // ============================
        // Experience
        // ============================
        [BsonElement("minExperience")]
        public int MinExperience { get; set; }

        [BsonElement("maxExperience")]
        public int MaxExperience { get; set; }

        // ============================
        // Location
        // ============================
        [BsonElement("city")]
        public string City { get; set; } = null!;

        [BsonElement("state")]
        public string State { get; set; } = null!;

        [BsonElement("country")]
        public string Country { get; set; } = null!;

        // ============================
        // Salary
        // ============================
        [BsonElement("minSalary")]
        public decimal MinSalary { get; set; }

        [BsonElement("maxSalary")]
        public decimal MaxSalary { get; set; }

        [BsonElement("currency")]
        public string Currency { get; set; } = "INR";

        [BsonElement("salaryFrequency")]
        public string SalaryFrequency { get; set; } = "Yearly";

        // ============================
        // Skills & Education
        // ============================
        [BsonElement("skills")]
        public List<string> Skills { get; set; } = new();

        [BsonElement("education")]
        public string Education { get; set; } = null!;

        [BsonElement("industry")]
        public string Industry { get; set; } = null!;

        // ============================
        // Job Meta
        // ============================
        [BsonElement("openings")]
        public int Openings { get; set; }

        [BsonElement("postedAt")]
        public DateTime PostedAt { get; set; }

        [BsonElement("expiryDate")]
        public DateTime? ExpiryDate { get; set; }

        [BsonElement("status")]
        public string Status { get; set; } = "Active";
    }
}
