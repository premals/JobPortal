using JobSeekerService.Application.Interfaces;
using JobSeekerService.Domain.Constants;
using JobSeekerService.Domain.Entities;
using static Shared.Contracts.Events.JobEvents;

namespace JobSeekerService.Application.EventHandler
{
    public class JobApplicationStatusUpdatedEventHandler
    {
        private readonly IJobApplicationRepository _applications;
        private readonly IJobReadRepository _jobs;
        private readonly INotificationRepository _notifications;

        public JobApplicationStatusUpdatedEventHandler(
            IJobApplicationRepository applications,
            IJobReadRepository jobs,
            INotificationRepository notifications)
        {
            _applications = applications;
            _jobs = jobs;
            _notifications = notifications;
        }

        public async Task HandleAsync(JobApplicationStatusUpdatedEvent evt)
        {
            var application = await _applications.GetAsync(evt.JobId, evt.JobSeekerId);
            if (application == null)
                return;

            if (!string.Equals(application.Status, evt.Status, StringComparison.OrdinalIgnoreCase))
            {
                application.Status = evt.Status;
                await _applications.UpdateAsync(application);
            }

            if (!string.Equals(evt.Status, ApplicationStatus.Shortlisted, StringComparison.OrdinalIgnoreCase))
                return;

            var job = await _jobs.GetByIdAsync(evt.JobId);
            var jobTitle = job?.Title ?? "a job you applied for";

            var notification = new Notification
            {
                JobSeekerId = evt.JobSeekerId,
                JobId = evt.JobId,
                Type = "Shortlisted",
                Title = "You are shortlisted",
                Message = $"You have been shortlisted for {jobTitle}."
            };

            await _notifications.AddAsync(notification);
        }
    }
}
