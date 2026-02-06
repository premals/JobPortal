using JobProviderService.Infrastructure.Messaging;
using static Shared.Contracts.Events.JobEvents;

namespace JobProviderService.Application.UseCases
{
    public class DeleteJobUseCase
    {
        private readonly IJobRepository _repository;
        private readonly IEventBus _eventBus;
        public DeleteJobUseCase(IJobRepository repository, IEventBus eventBus)
        {
            _repository = repository;
            _eventBus = eventBus;
        }

        public async Task ExecuteAsync(string jobId, string providerId)
        {
            var job = await _repository.GetByIdAsync(jobId);

            if (job == null)
                throw new InvalidOperationException("Job not found");

            if (job.JobProviderId != providerId)
                throw new UnauthorizedAccessException("You are not allowed to delete this job");

            // Soft delete (close job)
            job.Status = "Closed";

            await _repository.UpdateAsync(job);

            // ---------------------------
            // 4. Publish event AFTER DB commit
            // ---------------------------
            await _eventBus.PublishAsync(new JobClosedEvent
            {
                JobId = job.Id,
                JobProviderId = job.JobProviderId,
                ClosedAt = DateTime.UtcNow
            });
        }
    }
}
