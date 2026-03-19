using JobSeekerService.Application.DTOs;
using JobSeekerService.Application.Interfaces;
using JobSeekerService.Domain.Entities;
using JobSeekerService.Infrastructure.Messaging;
using static Shared.Contracts.Events.JobEvents;

namespace JobSeekerService.Application.UseCases
{
    public class ApplyJobUseCase
    {
        private readonly IJobApplicationRepository _repo;
        private readonly IJobReadRepository _jobReadRepository;
        private readonly IEventBus _eventBus;

        public ApplyJobUseCase(
            IJobApplicationRepository repo,
            IJobReadRepository jobReadRepository,
            IEventBus eventBus)
        {
            _repo = repo;
            _jobReadRepository = jobReadRepository;
            _eventBus = eventBus;
        }

        public async Task ExecuteAsync(ApplyJobRequest request, JobSeekerProfile seeker)
        {
            if (string.IsNullOrWhiteSpace(request.JobId))
                throw new ApplicationException("JobId is required.");

            var existing = await _repo.GetAsync(request.JobId, seeker.UserId);
            if (existing != null && !string.Equals(existing.Status, "Withdrawn", StringComparison.OrdinalIgnoreCase))
                throw new ApplicationException("You have already applied for this job.");

            var snapshot = await _jobReadRepository.GetByIdAsync(request.JobId);
            if (snapshot == null)
                throw new InvalidOperationException("Job not found.");

            if (!string.Equals(snapshot.Status, "Active", StringComparison.OrdinalIgnoreCase))
                throw new ApplicationException("This job is no longer accepting applications.");

            var providerId = string.IsNullOrWhiteSpace(request.JobProviderId)
                ? snapshot.JobProviderId
                : request.JobProviderId;

            var app = new JobApplication
            {
                JobId = request.JobId,
                JobSeekerId = seeker.UserId,
                JobProviderId = providerId!
            };

            await _repo.ApplyAsync(app);

            await _eventBus.PublishAsync(new JobAppliedEvent
            {
                JobId = request.JobId,
                JobProviderId = providerId!,
                JobSeekerId = seeker.UserId,
                FullName = seeker.FullName,
                Email = seeker.Email,
                Phone = seeker.Phone ?? string.Empty,
                ResumeUrl = request.ResumeUrl ?? string.Empty,
                AppliedAt = DateTime.Now,
                Status = "Applied"
            });
        }
    }
}
