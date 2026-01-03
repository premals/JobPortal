using IdendityService.Interfaces.Auth;
using IdendityService.Models;
using Microsoft.AspNetCore.Identity;

namespace IdendityService.Services.UseCases
{
    public class ConfirmEmailUseCase : IConfirmEmailUseCase
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public ConfirmEmailUseCase(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task ConfirmAsync(string userId, string token)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                throw new ApplicationException("Invalid user");

            var result = await _userManager.ConfirmEmailAsync(user, token);
            if (!result.Succeeded)
                throw new ApplicationException("Email confirmation failed");
        }
    }
}
