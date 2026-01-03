using IdendityService.Models;
using System.Security.Claims;

namespace IdendityService.Interfaces
{
    public interface IJwtService
    {
        string GenerateAccessToken(ApplicationUser user, IEnumerable<string> roles, out DateTime expiresAt);
        RefreshToken GenerateRefreshToken(string ipAddress);
        ClaimsPrincipal? ValidatePrincipalFromExpiredToken(string token);
    }
}
