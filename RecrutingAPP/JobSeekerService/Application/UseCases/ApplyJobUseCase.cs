using Azure.Core;
using JobSeekerService.Application.DTOs;
using JobSeekerService.Application.Interfaces;
using JobSeekerService.Domain.Entities;
using JobSeekerService.Infrastructure.Messaging;
using static Shared.Contracts.Events.JobEvents;
using static System.Net.Mime.MediaTypeNames;

namespace JobSeekerService.Application.UseCases
{
    public class ApplyJobUseCase
    {
        private readonly IJobApplicationRepository _repo;
        private readonly IEventBus _eventBus;

        public ApplyJobUseCase(IJobApplicationRepository repo, IEventBus eventBus)
        {
            _repo = repo;
            _eventBus = eventBus;
        }

        public async Task ExecuteAsync(ApplyJobRequest request, JobSeekerProfile seeker)
        {
            var app = new JobApplication
            {
                JobId = request.JobId,
                JobSeekerId = seeker.UserId
            };

            await _repo.ApplyAsync(app);

            await _eventBus.PublishAsync(new JobAppliedEvent
            {
                JobId = request.JobId,
                JobProviderId = request.JobProviderId,
                JobSeekerId = seeker.UserId,

                FullName = seeker.FullName,
                Email = seeker.Email,
                //Phone = seeker.Phone,

                ResumeUrl = request.ResumeUrl,
                AppliedAt = DateTime.Now,
                Status = "Applied"
            });
        }
    }
}
