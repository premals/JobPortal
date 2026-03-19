namespace IdendityService.DTOs
{
    public class Reset<secret>RequestDto
    {
        public record Reset<secret>Request(string Email, string Token, string New<secret>);

    }
}
