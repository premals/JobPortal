using JobProviderService.DTO;
using JobProviderService.Infrastructure.Messaging;
using static Shared.Contracts.Events.JobEvents;

namespace JobProviderService.Application.UseCases
{
    public class UpdateJobPartialUseCase
    {
        private readonly IJobRepository _repository;
        private readonly IEventBus _eventBus;
        public UpdateJobPartialUseCase(IJobRepository repository, IEventBus eventBus)
        {
            _repository = repository;
            _eventBus = eventBus;
        }

        public async Task ExecuteAsync(
            string jobId,
            string providerId,
            UpdateJobPatchRequest request)
        {
            if (request.MinSalary.HasValue && request.MaxSalary.HasValue &&
                request.MinSalary > request.MaxSalary)
                throw new ArgumentException("Min salary cannot exceed max salary");

            if (request.MinExperience.HasValue && request.MaxExperience.HasValue &&
                request.MinExperience > request.MaxExperience)
                throw new ArgumentException("Min experience cannot exceed max experience");

            await _repository.UpdatePartialAsync(jobId, providerId, request);

            await _eventBus.PublishAsync(new JobUpdatedEvent
            {
                JobId = jobId,
                JobProviderId = providerId,

                Title = request.Title,
                Description = request.Description,
                EmploymentType = request.EmploymentType,
                WorkMode = request.WorkMode,

                MinExperience = request.MinExperience,
                MaxExperience = request.MaxExperience,

                City = request.City,
                State = request.State,
                Country = request.Country,

                MinSalary = request.MinSalary,
                MaxSalary = request.MaxSalary,
                Currency = request.Currency,
                SalaryFrequency = request.SalaryFrequency,

                KeySkills = request.KeySkills,
                Education = request.Education,
                Industry = request.Industry,

                Openings = request.Openings,
                ExpiryDate = request.ExpiryDate
            });
        }
    }
}
