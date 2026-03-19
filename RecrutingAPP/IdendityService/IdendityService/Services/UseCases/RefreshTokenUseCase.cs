using IdendityService.Interfaces;
using IdendityService.Interfaces.Auth;
using IdendityService.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace IdendityService.Services.UseCases
{
public class RefreshTokenUseCase (UserManager<ApplicationUser> _userManager, IJwtService _jwtService, IRefreshTokenService _refreshTokenService) : IRefreshTokenUseCase
{

        public async Task<DTOs.AuthResponseDto.AuthResponse> RefreshAsync(string token, string ip)
        {
            var user = (await _userManager.Users.ToListAsync())
                .FirstOrDefault(u => u.RefreshTokens.Any(t => t.Token == token));

            if (user == null)
                throw new UnauthorizedAccessException("Invalid refresh token");

            // 🔒 Revoke old refresh token
            await _refreshTokenService.RevokeRefreshTokenAsync(user, token);

            // 🔑 Generate new tokens
            var roles = await _userManager.GetRolesAsync(user);

            var accessToken = _jwtService.GenerateAccessToken(
                user,
                roles,
                out var expiresAt
            );

            var newRefreshToken = _jwtService.GenerateRefreshToken(ip);

            await _refreshTokenService.AddRefreshTokenAsync(user, newRefreshToken);

            // ✅ Build profile (same as LoginAsync)
            var profile = new DTOs.AuthResponseDto.UserProfile(
                user.Id.ToString(),
                user.FullName,
                user.Email!,
                roles.FirstOrDefault() ?? "JobSeeker",
                user.Force<secret>Reset
            );

            return new DTOs.AuthResponseDto.AuthResponse(
                accessToken,
                newRefreshToken.Token,
                expiresAt,
                profile
            );
        }

    }
}
