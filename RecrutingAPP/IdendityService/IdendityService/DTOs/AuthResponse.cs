namespace IdendityService.DTOs
{
    public class AuthResponseDto
    {
        public record AuthResponse(
            string AccessToken,
            string RefreshToken,
            DateTime AccessTokenExpiresAt,
            UserProfile Profile
        );

        public record UserProfile(
            string UserId,
            string FullName,
            string Email,
            string UserType,
            bool ForcePasswordReset
        );
    }
}
