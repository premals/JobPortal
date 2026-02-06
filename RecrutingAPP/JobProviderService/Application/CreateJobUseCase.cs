using JobProviderService.Domain;
using JobProviderService.Infrastructure.Messaging;
using static Shared.Contracts.Events.JobEvents;

namespace JobProviderService.Application
{
    public class CreateJobUseCase
    {
        private readonly IJobRepository _repository;
        private readonly IEventBus _eventBus;
        public CreateJobUseCase(IJobRepository repository, IEventBus eventBus)
        {
            _repository = repository;
            _eventBus = eventBus;
        }

        public async Task ExecuteAsync(string providerId, CreateJobRequest request)
        {
            var job = new Job
            {
                // Identity
                JobProviderId = providerId,

                // Basic Job Info
                Title = request.Title,
                Description = request.Description,
                EmploymentType = request.EmploymentType,
                WorkMode = request.WorkMode,

                // Experience
                MinExperience = request.MinExperience,
                MaxExperience = request.MaxExperience,

                // Location
                City = request.City,
                State = request.State,
                Country = request.Country,

                // Salary
                MinSalary = request.MinSalary,
                MaxSalary = request.MaxSalary,
                Currency = request.Currency,
                SalaryFrequency = request.SalaryFrequency,

                // Skills & Education
                KeySkills = request.KeySkills,
                Education = request.Education,
                Industry = request.Industry,

                // Job Meta
                Openings = request.Openings,
                ExpiryDate = request.ExpiryDate,

                // Defaults
                Status = "Active",
                PostedAt = DateTime.UtcNow
            };

            bool restlt = await _repository.CreateAsync(job);


            if (restlt)
            {
                await _eventBus.PublishAsync(new JobCreatedEvent
                {
                    // ============================
                    // Job Identity
                    // ============================
                    JobId = job.Id,
                    JobProviderId = job.JobProviderId,

                    // ============================
                    // Basic Job Info
                    // ============================
                    Title = job.Title,
                    Description = job.Description,
                    EmploymentType = job.EmploymentType,
                    WorkMode = job.WorkMode,

                    // ============================
                    // Experience
                    // ============================
                    MinExperience = job.MinExperience,
                    MaxExperience = job.MaxExperience,

                    // ============================
                    // Location
                    // ============================
                    City = job.City,
                    State = job.State,
                    Country = job.Country,

                    // ============================
                    // Salary
                    // ============================
                    MinSalary = job.MinSalary,
                    MaxSalary = job.MaxSalary,
                    Currency = job.Currency,
                    SalaryFrequency = job.SalaryFrequency,

                    // ============================
                    // Skills & Education
                    // ============================
                    KeySkills = job.KeySkills,
                    Education = job.Education,
                    Industry = job.Industry,

                    // ============================
                    // Job Meta
                    // ============================
                    Openings = job.Openings,
                    PostedAt = job.PostedAt,
                    ExpiryDate = job.ExpiryDate,
                    Status = job.Status
                });
            }
        }
    }
}
