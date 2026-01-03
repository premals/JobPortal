using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace JobSeekerService.Domain.Entities
{
    public class JobApplication
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = null!;

        public string JobId { get; set; } = null!;
        public string JobSeekerId { get; set; } = null!;
        public string Status { get; set; } = "Applied";

        public DateTime AppliedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
