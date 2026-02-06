using JobProviderService.Application.Interfaces;
using JobProviderService.Infrastructure.Messaging;
using System.Linq;
using static Shared.Contracts.Events.JobEvents;

namespace JobProviderService.Application.UseCases
{
    public class UpdateApplicationStatusUseCase
    {
        private readonly IJobApplicationRepository _applications;
        private readonly IEventBus _eventBus;
        private static readonly string[] AllowedStatuses =
        {
            "Applied",
            "Shortlisted",
            "Rejected",
            "Hired"
        };

        public UpdateApplicationStatusUseCase(
            IJobApplicationRepository applications,
            IEventBus eventBus)
        {
            _applications = applications;
            _eventBus = eventBus;
        }

        public async Task ExecuteAsync(
            string jobId,
            string jobSeekerId,
            string providerId,
            string status)
        {
            if (string.IsNullOrWhiteSpace(status))
                throw new ArgumentException("Status is required");

            var normalized = status.Trim();
            var canonical = AllowedStatuses
                .FirstOrDefault(s => s.Equals(normalized, StringComparison.OrdinalIgnoreCase));

            if (canonical == null)
                throw new ArgumentException("Invalid status value");

            var application = await _applications.GetAsync(jobId, jobSeekerId);
            if (application == null)
                throw new InvalidOperationException("Application not found");

            if (application.JobProviderId != providerId)
                throw new UnauthorizedAccessException("You are not allowed to update this application");

            if (string.Equals(application.Status, canonical, StringComparison.OrdinalIgnoreCase))
                return;

            await _applications.UpdateAsync(jobId, jobSeekerId, canonical);

            await _eventBus.PublishAsync(new JobApplicationStatusUpdatedEvent
            {
                JobId = jobId,
                JobProviderId = providerId,
                JobSeekerId = jobSeekerId,
                Status = canonical,
                UpdatedAt = DateTime.UtcNow
            });
        }
    }
}
