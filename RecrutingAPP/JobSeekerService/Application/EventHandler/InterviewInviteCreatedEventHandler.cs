using JobSeekerService.Application.Interfaces;
using JobSeekerService.Domain.Entities;
using static Shared.Contracts.Events.JobEvents;

namespace JobSeekerService.Application.EventHandler
{
    public class InterviewInviteCreatedEventHandler
    {
        private readonly INotificationRepository _notifications;

        public InterviewInviteCreatedEventHandler(INotificationRepository notifications)
        {
            _notifications = notifications;
        }

        public async Task HandleAsync(InterviewInviteCreatedEvent evt)
        {
            var notification = new Notification
            {
                JobSeekerId = evt.JobSeekerId,
                JobId = evt.JobId,
                Type = "InterviewInvite",
                Title = "Interview invitation",
                Message = $"You have an interview invite for {evt.JobTitle}. Please choose a time slot."
            };

            await _notifications.AddAsync(notification);
        }
    }
}
