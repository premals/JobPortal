using JobProviderService.Application.Interfaces;

namespace JobProviderService.Infrastructure.Services
{
    public class ConsoleEmailService : IEmailService
    {
        public Task SendAsync(string to, string subject, string html)
        {
            Console.WriteLine($"EMAIL to: {to} subject:{subject}");
            return Task.CompletedTask;
        }
    }
}
