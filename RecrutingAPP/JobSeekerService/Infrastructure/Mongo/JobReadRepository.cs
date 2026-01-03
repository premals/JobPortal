using JobSeekerService.Application.DTOs;
using JobSeekerService.Application.Interfaces;
using JobSeekerService.Domain.Entities;
using MongoDB.Driver;
using static Shared.Contracts.Events.JobEvents;

namespace JobSeekerService.Infrastructure.Mongo
{
    public class JobReadRepository : IJobReadRepository
    {
        private readonly IMongoCollection<JobSnapshot> _collection;

        public JobReadRepository(MongoDbContext context)
        {
            _collection = context.Jobs;
        }

        // =====================================================
        // GET ALL JOBS (PAGINATED)
        // =====================================================
        public async Task<List<JobSnapshot>> GetAllAsync(int page, int pageSize)
        {
            page = page <= 0 ? 1 : page;
            pageSize = pageSize <= 0 ? 20 : pageSize;

            return await _collection
                .Find(j => j.Status == "Active")
                .SortByDescending(j => j.PostedAt)
                .Skip((page - 1) * pageSize)
                .Limit(pageSize)
                .ToListAsync();
        }

        // =====================================================
        // GET JOB BY ID
        // =====================================================
        public async Task<JobSnapshot?> GetByIdAsync(string jobId)
        {
            if (string.IsNullOrWhiteSpace(jobId))
                return null;

            return await _collection
                .Find(j => j.JobId == jobId && j.Status == "Active")
                .FirstOrDefaultAsync();
        }

        // =====================================================
        // SEARCH JOBS (KEYWORD + FILTERS)
        // =====================================================
        public async Task<List<JobSnapshot>> SearchAsync(JobSearchRequest request)
        {
            var builder = Builders<JobSnapshot>.Filter;
            var filters = new List<FilterDefinition<JobSnapshot>>
        {
            builder.Eq(j => j.Status, "Active")
        };

            // 🔍 Keyword (Title / Company)
            if (!string.IsNullOrWhiteSpace(request.Keyword))
            {
                filters.Add(builder.Text(request.Keyword));
            }

            // 📍 City
            if (!string.IsNullOrWhiteSpace(request.City))
            {
                filters.Add(builder.Eq(j => j.City, request.City));
            }

            // 💼 Employment Type
            if (!string.IsNullOrWhiteSpace(request.EmploymentType))
            {
                filters.Add(builder.Eq(j => j.EmploymentType, request.EmploymentType));
            }

            // 💰 Minimum Salary
            if (request.MinSalary.HasValue)
            {
                filters.Add(builder.Gte(j => j.MaxSalary, request.MinSalary.Value));
            }

            // 👨‍💻 Minimum Experience
            if (request.MinExperience.HasValue)
            {
                filters.Add(builder.Lte(j => j.MinExperience, request.MinExperience.Value));
            }

            return await _collection
                .Find(builder.And(filters))
                .SortByDescending(j => j.PostedAt)
                .Limit(100)
                .ToListAsync();
        }

        // =====================================================
        // GET JOBS BY SKILL
        // =====================================================
        public async Task<List<JobSnapshot>> GetBySkillAsync(string skill)
        {
            if (string.IsNullOrWhiteSpace(skill))
                return new List<JobSnapshot>();

            return await _collection
                .Find(j =>
                    j.Status == "Active" &&
                    j.Skills.Any(s => s.ToLower().Contains(skill.ToLower())))
                .SortByDescending(j => j.PostedAt)
                .Limit(50)
                .ToListAsync();
        }

        // =====================================================
        // GET JOBS BY LOCATION
        // =====================================================
        public async Task<List<JobSnapshot>> GetByLocationAsync(string city)
        {
            if (string.IsNullOrWhiteSpace(city))
                return new List<JobSnapshot>();

            return await _collection
                .Find(j => j.Status == "Active" && j.City == city)
                .SortByDescending(j => j.PostedAt)
                .Limit(50)
                .ToListAsync();
        }

        // =====================================================
        // GET SIMILAR JOBS
        // =====================================================
        public async Task<List<JobSnapshot>> GetSimilarAsync(string jobId)
        {
            var job = await GetByIdAsync(jobId);
            if (job == null)
                return new List<JobSnapshot>();

            return await _collection
                .Find(j =>
                    j.Status == "Active" &&
                    j.JobId != job.JobId &&
                    j.EmploymentType == job.EmploymentType &&
                    j.City == job.City)
                .SortByDescending(j => j.PostedAt)
                .Limit(10)
                .ToListAsync();
        }

        // =====================================================
        // UPSERT JOB FROM JobCreatedEvent
        // =====================================================
        public async Task UpsertFromEventAsync(JobCreatedEvent e)
        {
            var snapshot = new JobSnapshot
            {
                JobId = e.JobId,
                JobProviderId = e.JobProviderId,

                Title = e.Title,
                Description = e.Description,
                EmploymentType = e.EmploymentType,
                WorkMode = e.WorkMode,

                MinExperience = e.MinExperience,
                MaxExperience = e.MaxExperience,

                City = e.City,
                State = e.State,
                Country = e.Country,

                MinSalary = e.MinSalary,
                MaxSalary = e.MaxSalary,
                Currency = e.Currency,
                SalaryFrequency = e.SalaryFrequency,

                Skills = e.KeySkills,
                Education = e.Education,
                Industry = e.Industry,

                Openings = e.Openings,
                PostedAt = e.PostedAt,
                ExpiryDate = e.ExpiryDate,
                Status = e.Status
            };

            try
            {
                await _collection.InsertOneAsync(snapshot);
            }
            catch (MongoWriteException ex)
                when (ex.WriteError.Category == ServerErrorCategory.DuplicateKey)
            {
                // Idempotency: event already processed
                // Safe to ignore
            }
        }

        public async Task MarkClosedAsync(string jobId)
        {
            await _collection.UpdateOneAsync(
                j => j.JobId == jobId,
                Builders<JobSnapshot>.Update
                    .Set(j => j.Status, "Closed"));
        }

        public async Task ApplyPartialUpdateAsync(JobUpdatedEvent e)
        {
            var updates = new List<UpdateDefinition<JobSnapshot>>();
            var builder = Builders<JobSnapshot>.Update;

            if (e.Title != null)
                updates.Add(builder.Set(j => j.Title, e.Title));

            if (e.Description != null)
                updates.Add(builder.Set(j => j.Description, e.Description));

            if (e.EmploymentType != null)
                updates.Add(builder.Set(j => j.EmploymentType, e.EmploymentType));

            if (e.WorkMode != null)
                updates.Add(builder.Set(j => j.WorkMode, e.WorkMode));

            if (e.MinExperience.HasValue)
                updates.Add(builder.Set(j => j.MinExperience, e.MinExperience.Value));

            if (e.MaxExperience.HasValue)
                updates.Add(builder.Set(j => j.MaxExperience, e.MaxExperience.Value));

            if (e.City != null)
                updates.Add(builder.Set(j => j.City, e.City));

            if (e.State != null)
                updates.Add(builder.Set(j => j.State, e.State));

            if (e.Country != null)
                updates.Add(builder.Set(j => j.Country, e.Country));

            if (e.MinSalary.HasValue)
                updates.Add(builder.Set(j => j.MinSalary, e.MinSalary.Value));

            if (e.MaxSalary.HasValue)
                updates.Add(builder.Set(j => j.MaxSalary, e.MaxSalary.Value));

            if (e.Currency != null)
                updates.Add(builder.Set(j => j.Currency, e.Currency));

            if (e.SalaryFrequency != null)
                updates.Add(builder.Set(j => j.SalaryFrequency, e.SalaryFrequency));

            if (e.KeySkills != null)
                updates.Add(builder.Set(j => j.Skills, e.KeySkills));

            if (e.Education != null)
                updates.Add(builder.Set(j => j.Education, e.Education));

            if (e.Industry != null)
                updates.Add(builder.Set(j => j.Industry, e.Industry));

            if (e.Openings.HasValue)
                updates.Add(builder.Set(j => j.Openings, e.Openings.Value));

            if (e.ExpiryDate.HasValue)
                updates.Add(builder.Set(j => j.ExpiryDate, e.ExpiryDate));

            if (!updates.Any())
                return;

            await _collection.UpdateOneAsync(
                j => j.JobId == e.JobId,
                builder.Combine(updates));
        }

        public async Task ReplaceFromEventAsync(JobFullyUpdatedEvent e)
        {
            var snapshot = new JobSnapshot
            {
                JobId = e.JobId,
                JobProviderId = e.JobProviderId,

                Title = e.Title,
                Description = e.Description,
                EmploymentType = e.EmploymentType,
                WorkMode = e.WorkMode,

                MinExperience = e.MinExperience,
                MaxExperience = e.MaxExperience,

                City = e.City,
                State = e.State,
                Country = e.Country,

                MinSalary = e.MinSalary,
                MaxSalary = e.MaxSalary,
                Currency = e.Currency,
                SalaryFrequency = e.SalaryFrequency,

                Skills = e.KeySkills,
                Education = e.Education,
                Industry = e.Industry,

                Openings = e.Openings,
                PostedAt = e.PostedAt,
                ExpiryDate = e.ExpiryDate,

                Status = e.Status
            };

            await _collection.ReplaceOneAsync(
                j => j.JobId == e.JobId,
                snapshot,
                new ReplaceOptions { IsUpsert = true });
        }
    }
}
