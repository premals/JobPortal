using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace JobProviderService.Domain
{
    public class JobApplication
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = null!;

        // ============================
        // Identity
        // ============================
        [BsonElement("jobId")]
        public string JobId { get; set; } = null!;

        [BsonElement("jobProviderId")]
        public string JobProviderId { get; set; } = null!;

        [BsonElement("jobSeekerId")]
        public string JobSeekerId { get; set; } = null!;

        // ============================
        // Candidate Snapshot
        // ============================
        [BsonElement("fullName")]
        public string FullName { get; set; } = null!;

        [BsonElement("email")]
        public string Email { get; set; } = null!;

        [BsonElement("phone")]
        public string Phone { get; set; } = null!;

        [BsonElement("resumeUrl")]
        public string ResumeUrl { get; set; } = null!;

        // ============================
        // Status
        // ============================
        [BsonElement("status")]
        public string Status { get; set; }

        // ============================
        // Audit
        // ============================
        [BsonElement("appliedAt")]
        public DateTime AppliedAt { get; set; }

        [BsonElement("updatedAt")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
