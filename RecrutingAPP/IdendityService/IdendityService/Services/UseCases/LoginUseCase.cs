using IdendityService.DTOs;
using IdendityService.Interfaces;
using IdendityService.Interfaces.Auth;
using IdendityService.Models;
using Microsoft.AspNetCore.Identity;

namespace IdendityService.Services.UseCases
{
    public class LoginUseCase : ILoginUseCase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IJwtService _jwtService;
        private readonly IRefreshTokenService _refreshTokenService;
        public LoginUseCase(
            UserManager<ApplicationUser> userManager,
            IJwtService jwtService,
            IRefreshTokenService refreshTokenService)
        {
            _userManager = userManager;
            _jwtService = jwtService;
            _refreshTokenService = refreshTokenService;
        }

        public async Task<AuthResponseDto.AuthResponse> LoginAsync(LoginRequest req, string ip)
        {
            var user = await _userManager.FindByEmailAsync(req.Email);

            if (user == null || !await _userManager.Check<secret>Async(user, req.<secret>))
                throw new UnauthorizedAccessException("Invalid credentials");

            if (!user.EmailConfirmed)
                throw new UnauthorizedAccessException("Please confirm your email before logging in.");

            // Self-heal legacy records where EmailConfirmed was set but EmailVerified was not.
            if (!user.EmailVerified)
            {
                user.EmailVerified = true;
                var updateResult = await _userManager.UpdateAsync(user);
                if (!updateResult.Succeeded)
                    throw new ApplicationException("Failed to sync email verification state.");
            }

            if (!user.IsActive)
                throw new UnauthorizedAccessException("Account is disabled. Contact admin.");

            var roles = await _userManager.GetRolesAsync(user);

            var accessToken = _jwtService.GenerateAccessToken(
                user,
                roles,
                out var expiresAt
            );

            var refreshToken = _jwtService.GenerateRefreshToken(ip);
            await _refreshTokenService.AddRefreshTokenAsync(user, refreshToken);

            var profile = new AuthResponseDto.UserProfile(
                user.Id.ToString(),
                user.FullName,
                user.Email!,
                roles.FirstOrDefault() ?? "JobSeeker",
                user.Force<secret>Reset
            );

            return new AuthResponseDto.AuthResponse(
                accessToken,
                refreshToken.Token,
                expiresAt,
                profile
            );
        }
    }
}
