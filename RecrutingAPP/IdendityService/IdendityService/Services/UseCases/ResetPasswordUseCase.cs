using IdendityService.Interfaces.Auth;
using IdendityService.Models;
using Microsoft.AspNetCore.Identity;

namespace IdendityService.Services.UseCases
{
    public class Reset<secret>UseCase : IReset<secret>UseCase
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public Reset<secret>UseCase(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task ExecuteAsync(IdendityService.DTOs.Reset<secret>RequestDto.Reset<secret>Request req)
        {
            var user = await _userManager.FindByEmailAsync(req.Email);
            if (user == null)
                throw new ApplicationException("Invalid reset request");

            var result = await _userManager.Reset<secret>Async(
                user,
                req.Token,
                req.New<secret>);

            if (!result.Succeeded)
                throw new ApplicationException("<secret> reset failed");
        }
    }
}
