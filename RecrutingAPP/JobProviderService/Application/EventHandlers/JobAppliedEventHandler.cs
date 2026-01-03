using JobProviderService.Application.Interfaces;
using JobProviderService.Domain;
using static Shared.Contracts.Events.JobEvents;

namespace JobProviderService.Application.EventHandlers
{
    public class JobAppliedEventHandler
    {
        private readonly IJobApplicationRepository _repository;

        public JobAppliedEventHandler(IJobApplicationRepository repository)
        {
            _repository = repository;
        }

        public async Task HandleAsync(JobAppliedEvent evt)
        {
            var exists = await _repository
                .GetAsync(evt.JobId, evt.JobSeekerId);

            if (exists != null)
                return; // idempotent

            await _repository.ApplyAsync(new JobApplication
            {
                JobId = evt.JobId,
                JobProviderId = evt.JobProviderId,
                JobSeekerId = evt.JobSeekerId,
                FullName = evt.FullName,
                Email = evt.Email,
                Phone = evt.Phone,
                ResumeUrl = evt.ResumeUrl,
                AppliedAt = evt.AppliedAt,
                Status = evt.Status
                
            });
        }
    }
}
