using JobProviderService.Application.Interfaces;
using static Shared.Contracts.Events.JobEvents;

namespace JobProviderService.Application.EventHandlers
{
    public class JobApplicationWithdrawnEventHandler
    {
        private readonly IJobApplicationRepository _repository;

        public JobApplicationWithdrawnEventHandler(
            IJobApplicationRepository repository)
        {
            _repository = repository;
        }

        public async Task HandleAsync(JobApplicationWithdrawnEvent evt)
        {
            var application = await _repository
                .GetAsync(evt.JobId, evt.JobSeekerId);

            if (application == null)
                return; // idempotent

            if (application.Status == "Withdrawn")
                return;

            application.Status = "Withdrawn";
            application.UpdatedAt = DateTime.UtcNow;

            await _repository.UpdateAsync(evt.JobId, evt.JobSeekerId,application.Status);
        }
    }
}
