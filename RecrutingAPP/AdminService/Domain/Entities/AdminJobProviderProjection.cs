using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace AdminService.Domain.Entities
{
    public class AdminJobProviderProjection
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = null!;

        public string JobProviderId { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public string BrandName { get; set; } = string.Empty;
        public string Industry { get; set; } = string.Empty;
        public string CompanySize { get; set; } = string.Empty;
        public string Website { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string About { get; set; } = string.Empty;
        public string? LogoUrl { get; set; }
        public string? LinkedInUrl { get; set; }
        public string? TwitterUrl { get; set; }

        public string ContactName { get; set; } = string.Empty;
        public string ContactEmail { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;
    }
}
