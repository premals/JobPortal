namespace IdendityService.DTOs
{
    public class ResetPasswordRequestDto
    {
        public record ResetPasswordRequest(string Email, string Token, string NewPassword);

    }
}
