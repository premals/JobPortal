using AspNetCore.Identity.MongoDbCore.Models;
using MongoDbGenericRepository.Attributes;

namespace IdendityService.Models
{
    [CollectionName("Users")]
    public class ApplicationUser : MongoIdentityUser<Guid>
    {
        public string FullName { get; set; }
        public bool EmailVerified { get; set; }
        public List<RefreshToken> RefreshTokens { get; set; } = new();
        public bool IsActive { get; set; } = true;
        public bool ForcePasswordReset { get; set; } = false;
    }
}
