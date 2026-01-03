using JobSeekerService.Application.Interfaces;
using JobSeekerService.Domain.Entities;
using static Shared.Contracts.Events.JobEvents;

namespace JobSeekerService.Application.EventHandler
{
    public class JobSeekerRegisteredEventHandler
    {
        private readonly IJobSeekerRepository _repository;

        public JobSeekerRegisteredEventHandler(
            IJobSeekerRepository repository)
        {
            _repository = repository;
        }

        public async Task HandleAsync(JobSeekerRegisteredEvent evt)
        {
            // 🔐 Idempotency (VERY IMPORTANT)
            var existing = await _repository.GetByUserIdAsync(evt.UserId);
            if (existing != null)
                return;

            var profile = new JobSeekerProfile
            {
                UserId = evt.UserId,
                FullName = evt.FullName,
                Email = evt.Email
            };

            await _repository.CreateAsync(profile);
        }
    }
}
