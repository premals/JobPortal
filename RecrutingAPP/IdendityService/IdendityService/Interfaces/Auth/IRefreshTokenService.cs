using IdendityService.Models;

namespace IdendityService.Interfaces.Auth
{
    public interface IRefreshTokenService
    {
        Task AddRefreshTokenAsync(ApplicationUser user, RefreshToken token);
        Task RevokeRefreshTokenAsync(ApplicationUser user, string token);
        Task<RefreshToken?> GetRefreshTokenAsync(ApplicationUser user, string token);
    }
}
