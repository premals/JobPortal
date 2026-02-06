using JobSeekerService.Infrastructure.Messaging;
using static Shared.Contracts.Events.JobEvents;

namespace JobSeekerService.Application.UseCases
{
    public class WithdrawJobApplicationUseCase
    {
        private readonly IEventBus _eventBus;

        public WithdrawJobApplicationUseCase(IEventBus eventBus)
        {
            _eventBus = eventBus;
        }

        public async Task ExecuteAsync(string jobId, string jobSeekerId, string jobProviderId)
        {
            await _eventBus.PublishAsync(new JobApplicationWithdrawnEvent
            {
                JobId = jobId,
                JobProviderId = jobProviderId,
                JobSeekerId = jobSeekerId
            });
        }
    }
}
