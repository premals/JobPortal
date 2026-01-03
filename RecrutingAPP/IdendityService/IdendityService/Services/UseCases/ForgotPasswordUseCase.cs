using IdendityService.Interfaces;
using IdendityService.Interfaces.Auth;
using IdendityService.Models;
using Microsoft.AspNetCore.Identity;

namespace IdendityService.Services.UseCases
{
    public class ForgotPasswordUseCase : IForgotPasswordUseCase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailService _emailService;

        public ForgotPasswordUseCase(
            UserManager<ApplicationUser> userManager,
            IEmailService emailService)
        {
            _userManager = userManager;
            _emailService = emailService;
        }

        public async Task ExecuteAsync(string email, string resetPasswordBaseUrl)
        {
            // SECURITY: Do not reveal whether user exists
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return;

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            var resetUrl =
                $"{resetPasswordBaseUrl}?email={Uri.EscapeDataString(email)}" +
                $"&token={Uri.EscapeDataString(token)}";

            await _emailService.SendAsync(
                user.Email!,
                "Reset your password",
                $"Click the link to reset password: {resetUrl}");
        }
    }
}
