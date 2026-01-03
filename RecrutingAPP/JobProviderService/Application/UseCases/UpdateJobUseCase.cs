using JobProviderService.DTO;
using JobProviderService.Infrastructure.Messaging;
using static Shared.Contracts.Events.JobEvents;

namespace JobProviderService.Application.UseCases
{
    public class UpdateJobUseCase
    {
        private readonly IJobRepository _repository;
        private readonly IEventBus _event;

        public UpdateJobUseCase(IJobRepository repository, IEventBus eventBus)
        {
            _repository = repository;
            _event = eventBus;
        }

        public async Task ExecuteAsync(
            string jobId,
            string providerId,
            UpdateJobRequest request)
        {
            var job = await _repository.GetByIdAsync(jobId);

            if (job == null)
                throw new InvalidOperationException("Job not found");

            if (job.JobProviderId != providerId)
                throw new UnauthorizedAccessException("You are not allowed to update this job");

            // -----------------------
            // Business Rules
            // -----------------------
            if (request.MinSalary > request.MaxSalary)
                throw new ArgumentException("Min salary cannot be greater than max salary");

            if (request.MinExperience > request.MaxExperience)
                throw new ArgumentException("Min experience cannot exceed max experience");

            // -----------------------
            // Update Fields
            // -----------------------
            job.Title = request.Title;
            job.Description = request.Description;
            job.EmploymentType = request.EmploymentType;
            job.WorkMode = request.WorkMode;

            job.MinExperience = request.MinExperience;
            job.MaxExperience = request.MaxExperience;

            job.City = request.City;
            job.State = request.State;
            job.Country = request.Country;

            job.MinSalary = request.MinSalary;
            job.MaxSalary = request.MaxSalary;
            job.Currency = request.Currency;
            job.SalaryFrequency = request.SalaryFrequency;

            job.KeySkills = request.KeySkills;
            job.Education = request.Education;
            job.Industry = request.Industry;

            job.Openings = request.Openings;
            job.ExpiryDate = request.ExpiryDate;

            await _repository.UpdateAsync(job);

            await _event.PublishAsync(new JobFullyUpdatedEvent
            {
                JobId = job.Id,
                JobProviderId = job.JobProviderId,

                Title = job.Title,
                Description = job.Description,
                EmploymentType = job.EmploymentType,
                WorkMode = job.WorkMode,

                MinExperience = job.MinExperience,
                MaxExperience = job.MaxExperience,

                City = job.City,
                State = job.State,
                Country = job.Country,

                MinSalary = job.MinSalary,
                MaxSalary = job.MaxSalary,
                Currency = job.Currency,
                SalaryFrequency = job.SalaryFrequency,

                KeySkills = job.KeySkills,
                Education = job.Education,
                Industry = job.Industry,

                Openings = job.Openings,
                PostedAt = job.PostedAt,
                ExpiryDate = job.ExpiryDate,

                Status = job.Status
            });
        }
    }
}
