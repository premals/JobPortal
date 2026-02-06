using JobSeekerService.Domain.Entities;

namespace JobSeekerService.Application.Interfaces
{
    public interface INotificationRepository
    {
        Task AddAsync(Notification notification);
        Task<List<Notification>> GetBySeekerAsync(string seekerId, int limit = 50);
        Task MarkReadAsync(string notificationId, string seekerId);
    }
}
