namespace IdendityService.DTOs
{
    public class Forgot<secret>RequestDto
    {
        public record Forgot<secret>Request(string Email);


    }
}
