namespace EmailService.Interfaces
{
    public interface IEmailSender
    {
        Task SendAsync(string to, string subject, string html);
    }
}
