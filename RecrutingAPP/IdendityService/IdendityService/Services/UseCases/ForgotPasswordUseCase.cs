using IdendityService.Interfaces;
using IdendityService.Interfaces.Auth;
using IdendityService.Models;
using Microsoft.AspNetCore.Identity;

namespace IdendityService.Services.UseCases
{
    public class Forgot<secret>UseCase : IForgot<secret>UseCase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailService _emailService;

        public Forgot<secret>UseCase(
            UserManager<ApplicationUser> userManager,
            IEmailService emailService)
        {
            _userManager = userManager;
            _emailService = emailService;
        }

        public async Task ExecuteAsync(string email, string reset<secret>BaseUrl)
        {
            // SECURITY: Do not reveal whether user exists
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return;

            var token = await _userManager.Generate<secret>ResetTokenAsync(user);

            var resetUrl =
                $"{reset<secret>BaseUrl}?email={Uri.EscapeDataString(email)}" +
                $"&token={Uri.EscapeDataString(token)}";

            await _emailService.SendAsync(
                user.Email!,
                "Reset your <secret>",
                $"Click the link to reset <secret>: {resetUrl}");
        }
    }
}
