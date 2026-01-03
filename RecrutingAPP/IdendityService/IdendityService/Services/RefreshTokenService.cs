using IdendityService.Interfaces.Auth;
using IdendityService.Models;
using Microsoft.AspNetCore.Identity;

namespace IdendityService.Services
{
    public class RefreshTokenService : IRefreshTokenService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        public RefreshTokenService(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }
        public async Task AddRefreshTokenAsync(ApplicationUser user, RefreshToken token)
        {
            user.RefreshTokens.Add(token);
            await _userManager.UpdateAsync(user);
        }

        public async Task<RefreshToken?> GetRefreshTokenAsync(ApplicationUser user, string token)
        {
            var rt = user.RefreshTokens.FirstOrDefault(x => x.Token == token);
            if (rt != null)
            {
                rt.Revoked = true;
                await _userManager.UpdateAsync(user);
            }

            return rt;
        }

        public Task RevokeRefreshTokenAsync(ApplicationUser user, string token)
        {
            throw new NotImplementedException();
        }
    }
}
