using AdminService.Domain.Entities;
using AdminService.Infrastructure.Repositories;
using MongoDB.Driver;
using System.Linq;
using static Shared.Contracts.Events.JobEvents;

namespace AdminService.Infrastructure.Messaging
{
    public class AdminEventHandler
    {
        private readonly AdminReadRepository _repository;

        public AdminEventHandler(AdminReadRepository repository)
        {
            _repository = repository;
        }

        public Task HandleAsync(JobSeekerRegisteredEvent evt)
        {
            if (string.IsNullOrWhiteSpace(evt.UserId))
                return Task.CompletedTask;

            return HandleJobSeekerRegisteredAsync(evt);
        }

        private async Task HandleJobSeekerRegisteredAsync(JobSeekerRegisteredEvent evt)
        {
            var exists = await _repository.JobSeekerExistsAsync(evt.UserId);
            if (exists)
                return;

            var projection = new AdminJobSeekerProjection
            {
                UserId = evt.UserId,
                FullName = evt.FullName,
                Email = evt.Email,
                CreatedAt = evt.OccurredAt,
                UpdatedAt = evt.OccurredAt,
                LastActiveAt = evt.OccurredAt
            };

            await _repository.UpsertJobSeekerAsync(projection);
        }

        public Task HandleAsync(JobProviderRegisteredEvent evt)
        {
            if (string.IsNullOrWhiteSpace(evt.UserId))
                return Task.CompletedTask;

            return HandleJobProviderRegisteredAsync(evt);
        }

        private async Task HandleJobProviderRegisteredAsync(JobProviderRegisteredEvent evt)
        {
            var exists = await _repository.JobProviderExistsAsync(evt.UserId);
            if (exists)
                return;

            var projection = new AdminJobProviderProjection
            {
                JobProviderId = evt.UserId,
                ContactName = evt.FullName,
                ContactEmail = evt.Email,
                CreatedAt = evt.OccurredAt,
                UpdatedAt = evt.OccurredAt
            };

            await _repository.UpsertJobProviderAsync(projection);
        }

        public Task HandleAsync(JobSeekerProfileUpsertedEvent evt)
        {
            var projection = new AdminJobSeekerProjection
            {
                UserId = evt.UserId,
                FullName = evt.FullName,
                Email = evt.Email,
                Phone = evt.Phone,
                Headline = evt.Headline,
                Summary = evt.Summary,
                Skills = evt.Skills,
                ExperienceYears = evt.ExperienceYears,
                Education = evt.Education,
                Location = evt.Location,
                WorkHistory = evt.WorkHistory.Select(item => new AdminWorkExperience
                {
                    Company = item.Company,
                    Role = item.Role,
                    StartDate = item.StartDate,
                    EndDate = item.EndDate,
                    Description = item.Description,
                    Skills = item.Skills
                }).ToList(),
                EducationHistory = evt.EducationHistory.Select(item => new AdminEducationRecord
                {
                    School = item.School,
                    Degree = item.Degree,
                    Field = item.Field,
                    GraduationYear = item.GraduationYear
                }).ToList(),
                Projects = evt.Projects.Select(item => new AdminProjectRecord
                {
                    Name = item.Name,
                    Role = item.Role,
                    Description = item.Description,
                    Link = item.Link
                }).ToList(),
                Certifications = evt.Certifications.Select(item => new AdminCertificationRecord
                {
                    Name = item.Name,
                    Issuer = item.Issuer,
                    Year = item.Year
                }).ToList(),
                Languages = evt.Languages.Select(item => new AdminLanguageRecord
                {
                    Name = item.Name,
                    Proficiency = item.Proficiency
                }).ToList(),
                ResumeSettings = new AdminResumeSettings
                {
                    AtsFriendly = evt.ResumeSettings.AtsFriendly,
                    Template = evt.ResumeSettings.Template
                },
                CreatedAt = evt.CreatedAt,
                UpdatedAt = evt.UpdatedAt,
                LastActiveAt = evt.UpdatedAt
            };

            return _repository.UpsertJobSeekerAsync(projection);
        }

        public Task HandleAsync(JobProviderProfileUpsertedEvent evt)
        {
            var projection = new AdminJobProviderProjection
            {
                JobProviderId = evt.JobProviderId,
                CompanyName = evt.CompanyName,
                BrandName = evt.BrandName,
                Industry = evt.Industry,
                CompanySize = evt.CompanySize,
                Website = evt.Website,
                Phone = evt.Phone,
                Location = evt.Location,
                About = evt.About,
                LogoUrl = evt.LogoUrl,
                LinkedInUrl = evt.LinkedInUrl,
                TwitterUrl = evt.TwitterUrl,
                ContactName = evt.ContactName,
                ContactEmail = evt.ContactEmail,
                CreatedAt = evt.CreatedAt,
                UpdatedAt = evt.UpdatedAt
            };

            return _repository.UpsertJobProviderAsync(projection);
        }

        public Task HandleAsync(JobCreatedEvent evt)
        {
            var projection = new AdminJobProjection
            {
                JobId = evt.JobId,
                JobProviderId = evt.JobProviderId,
                Title = evt.Title,
                Description = evt.Description,
                EmploymentType = evt.EmploymentType,
                WorkMode = evt.WorkMode,
                MinExperience = evt.MinExperience,
                MaxExperience = evt.MaxExperience,
                City = evt.City,
                State = evt.State,
                Country = evt.Country,
                MinSalary = evt.MinSalary,
                MaxSalary = evt.MaxSalary,
                Currency = evt.Currency,
                SalaryFrequency = evt.SalaryFrequency,
                KeySkills = evt.KeySkills,
                Education = evt.Education,
                Industry = evt.Industry,
                Openings = evt.Openings,
                PostedAt = evt.PostedAt,
                ExpiryDate = evt.ExpiryDate,
                Status = evt.Status
            };

            return _repository.UpsertJobAsync(projection);
        }

        public Task HandleAsync(JobUpdatedEvent evt)
        {
            var updates = new List<UpdateDefinition<AdminJobProjection>>();
            var updateBuilder = Builders<AdminJobProjection>.Update;

            if (evt.Title != null) updates.Add(updateBuilder.Set(x => x.Title, evt.Title));
            if (evt.Description != null) updates.Add(updateBuilder.Set(x => x.Description, evt.Description));
            if (evt.EmploymentType != null) updates.Add(updateBuilder.Set(x => x.EmploymentType, evt.EmploymentType));
            if (evt.WorkMode != null) updates.Add(updateBuilder.Set(x => x.WorkMode, evt.WorkMode));
            if (evt.MinExperience.HasValue) updates.Add(updateBuilder.Set(x => x.MinExperience, evt.MinExperience.Value));
            if (evt.MaxExperience.HasValue) updates.Add(updateBuilder.Set(x => x.MaxExperience, evt.MaxExperience.Value));
            if (evt.City != null) updates.Add(updateBuilder.Set(x => x.City, evt.City));
            if (evt.State != null) updates.Add(updateBuilder.Set(x => x.State, evt.State));
            if (evt.Country != null) updates.Add(updateBuilder.Set(x => x.Country, evt.Country));
            if (evt.MinSalary.HasValue) updates.Add(updateBuilder.Set(x => x.MinSalary, evt.MinSalary.Value));
            if (evt.MaxSalary.HasValue) updates.Add(updateBuilder.Set(x => x.MaxSalary, evt.MaxSalary.Value));
            if (evt.Currency != null) updates.Add(updateBuilder.Set(x => x.Currency, evt.Currency));
            if (evt.SalaryFrequency != null) updates.Add(updateBuilder.Set(x => x.SalaryFrequency, evt.SalaryFrequency));
            if (evt.KeySkills != null) updates.Add(updateBuilder.Set(x => x.KeySkills, evt.KeySkills));
            if (evt.Education != null) updates.Add(updateBuilder.Set(x => x.Education, evt.Education));
            if (evt.Industry != null) updates.Add(updateBuilder.Set(x => x.Industry, evt.Industry));
            if (evt.Openings.HasValue) updates.Add(updateBuilder.Set(x => x.Openings, evt.Openings.Value));
            if (evt.ExpiryDate.HasValue) updates.Add(updateBuilder.Set(x => x.ExpiryDate, evt.ExpiryDate));

            if (updates.Count == 0)
            {
                return Task.CompletedTask;
            }

            var updateDefinition = updateBuilder.Combine(updates);
            return _repository.ApplyJobPartialUpdateAsync(evt.JobId, updateDefinition);
        }

        public Task HandleAsync(JobFullyUpdatedEvent evt)
        {
            var projection = new AdminJobProjection
            {
                JobId = evt.JobId,
                JobProviderId = evt.JobProviderId,
                Title = evt.Title,
                Description = evt.Description,
                EmploymentType = evt.EmploymentType,
                WorkMode = evt.WorkMode,
                MinExperience = evt.MinExperience,
                MaxExperience = evt.MaxExperience,
                City = evt.City,
                State = evt.State,
                Country = evt.Country,
                MinSalary = evt.MinSalary,
                MaxSalary = evt.MaxSalary,
                Currency = evt.Currency,
                SalaryFrequency = evt.SalaryFrequency,
                KeySkills = evt.KeySkills,
                Education = evt.Education,
                Industry = evt.Industry,
                Openings = evt.Openings,
                PostedAt = evt.PostedAt,
                ExpiryDate = evt.ExpiryDate,
                Status = evt.Status
            };

            return _repository.UpsertJobAsync(projection);
        }

        public Task HandleAsync(JobClosedEvent evt)
        {
            return _repository.CloseJobAsync(evt.JobId, evt.ClosedAt);
        }

        public async Task HandleAsync(JobAppliedEvent evt)
        {
            var applicationId = BuildApplicationId(evt.JobId, evt.JobSeekerId);
            var projection = new AdminApplicationProjection
            {
                ApplicationId = applicationId,
                JobId = evt.JobId,
                JobProviderId = evt.JobProviderId,
                JobSeekerId = evt.JobSeekerId,
                CandidateName = evt.FullName,
                CandidateEmail = evt.Email,
                CandidatePhone = evt.Phone,
                ResumeUrl = evt.ResumeUrl,
                Status = evt.Status,
                AppliedAt = evt.AppliedAt,
                UpdatedAt = evt.AppliedAt
            };

            await _repository.UpsertApplicationAsync(projection);
            await _repository.UpdateJobSeekerLastActiveAsync(evt.JobSeekerId, evt.AppliedAt);
        }

        public async Task HandleAsync(JobApplicationStatusUpdatedEvent evt)
        {
            var applicationId = BuildApplicationId(evt.JobId, evt.JobSeekerId);
            await _repository.UpdateApplicationStatusAsync(applicationId, evt.Status, evt.UpdatedAt);
            await _repository.UpdateJobSeekerLastActiveAsync(evt.JobSeekerId, evt.UpdatedAt);
        }

        public async Task HandleAsync(JobApplicationWithdrawnEvent evt)
        {
            var applicationId = BuildApplicationId(evt.JobId, evt.JobSeekerId);
            await _repository.UpdateApplicationStatusAsync(applicationId, "Withdrawn", evt.OccurredAt);
            await _repository.UpdateJobSeekerLastActiveAsync(evt.JobSeekerId, evt.OccurredAt);
        }

        private static string BuildApplicationId(string jobId, string seekerId) => $"{jobId}:{seekerId}";
    }
}

