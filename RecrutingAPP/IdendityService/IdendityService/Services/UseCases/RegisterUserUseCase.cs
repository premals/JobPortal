using IdendityService.DTOs;
using IdendityService.Infrastructure.Messaging;
using IdendityService.Interfaces;
using IdendityService.Interfaces.Auth;
using IdendityService.Models;
using IdendityService.ResultModel;
using Microsoft.AspNetCore.Identity;
using MongoDB.Bson;
using static Shared.Contracts.Events.JobEvents;

namespace IdendityService.Services.UseCases
{
    public class RegisterUserUseCase : IRegisterUserUseCase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailService _emailService;
        private readonly IEventBus _eventBus;

        public RegisterUserUseCase(
            UserManager<ApplicationUser> userManager,
            IEmailService emailService,
            IEventBus eventBus)
        {
            _userManager = userManager;
            _emailService = emailService;
            _eventBus = eventBus;
        }

        public async Task<Result> RegisterAsync(
            RegisterRequest request,
            string confirmationBaseUrl)
        {
            var existingUser = await _userManager.FindByEmailAsync(request.Email);
            if (existingUser != null)
                return Result.Fail("Email already exists.");


            // 🔐 VALIDATE ROLE
            var allowedRoles = new[] { "JobProvider", "JobSeeker", "Admin" };
            if (!allowedRoles.Contains(request.UserType))
                return Result.Fail("Invalid user type.");

            var user = new ApplicationUser
            {
                UserName = request.Email,
                Email = request.Email,
                FullName = request.FullName
            };

            var createResult = await _userManager.CreateAsync(user, request.Password);
            if (!createResult.Succeeded)
                return Result.Fail("User creation failed.");

            await _userManager.AddToRoleAsync(user, request.UserType);

            await _eventBus.PublishAsync(new JobSeekerRegisteredEvent
            {
                UserId = user.Id.ToString(),
                FullName = user.FullName,
                Email = user.Email!
            });

            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var verifyUrl =
                $"{confirmationBaseUrl}?userId={user.Id}&token={Uri.EscapeDataString(token)}";

            await _emailService.SendAsync(
                user.Email!,
                "Confirm your email",
                $"Click to confirm: {verifyUrl}");

            return Result.Ok("Registered successfully. Please confirm your email.");
        }
    }
}
