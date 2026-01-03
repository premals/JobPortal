using IdendityService.Interfaces;

namespace IdendityService.Services
{
    public class EmailService : IEmailService
    {
        public Task SendAsync(string to, string subject, string html)
        {
            Console.WriteLine($"EMAIL to: {to} subject:{subject}");
            return Task.CompletedTask;
        }
    }
}
