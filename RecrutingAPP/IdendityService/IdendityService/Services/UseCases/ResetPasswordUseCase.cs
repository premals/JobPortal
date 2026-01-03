using IdendityService.Interfaces.Auth;
using IdendityService.Models;
using Microsoft.AspNetCore.Identity;

namespace IdendityService.Services.UseCases
{
    public class ResetPasswordUseCase : IResetPasswordUseCase
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public ResetPasswordUseCase(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task ExecuteAsync(IdendityService.DTOs.ResetPasswordRequestDto.ResetPasswordRequest req)
        {
            var user = await _userManager.FindByEmailAsync(req.Email);
            if (user == null)
                throw new ApplicationException("Invalid reset request");

            var result = await _userManager.ResetPasswordAsync(
                user,
                req.Token,
                req.NewPassword);

            if (!result.Succeeded)
                throw new ApplicationException("Password reset failed");
        }
    }
}
