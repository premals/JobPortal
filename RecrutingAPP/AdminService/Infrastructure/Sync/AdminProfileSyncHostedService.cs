using AdminService.Domain.Entities;
using AdminService.Infrastructure.Mongo;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;

namespace AdminService.Infrastructure.Sync
{
    public class AdminProfileSyncHostedService : BackgroundService
    {
        private const string JobSeekerCollectionName = "JobSeekers";
        private const string JobProviderCollectionName = "JobProviderProfiles";

        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IOptions<AdminSyncOptions> _options;
        private readonly ILogger<AdminProfileSyncHostedService> _logger;

        public AdminProfileSyncHostedService(
            IServiceScopeFactory scopeFactory,
            IOptions<AdminSyncOptions> options,
            ILogger<AdminProfileSyncHostedService> logger)
        {
            _scopeFactory = scopeFactory;
            _options = options;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var config = _options.Value;
            if (!config.Enabled)
            {
                _logger.LogInformation("Admin profile sync is disabled.");
                return;
            }

            if (config.RunOnStartup)
            {
                await RunSyncAsync(stoppingToken);
            }

            var interval = TimeSpan.FromMinutes(Math.Max(1, config.IntervalMinutes));
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(interval, stoppingToken);
                    await RunSyncAsync(stoppingToken);
                }
                catch (TaskCanceledException)
                {
                    // Ignore cancellation during delay.
                }
            }
        }

        private async Task RunSyncAsync(CancellationToken stoppingToken)
        {
            using var scope = _scopeFactory.CreateScope();
            var adminContext = scope.ServiceProvider.GetRequiredService<MongoDbContext>();

            await SyncJobSeekersAsync(adminContext, stoppingToken);
            await SyncJobProvidersAsync(adminContext, stoppingToken);
        }

        private async Task SyncJobSeekersAsync(MongoDbContext adminContext, CancellationToken token)
        {
            var settings = _options.Value.JobSeeker;
            if (!IsValid(settings))
            {
                _logger.LogWarning("Job seeker sync skipped: missing Mongo connection settings.");
                return;
            }

            var sourceCollection = GetSourceCollection<JobSeekerProfileSnapshot>(
                settings.ConnectionString,
                settings.DatabaseName,
                JobSeekerCollectionName);

            var existingIds = await adminContext.JobSeekers
                .Distinct(x => x.UserId, FilterDefinition<AdminJobSeekerProjection>.Empty)
                .ToListAsync(token);

            var existing = new HashSet<string>(existingIds);
            var inserted = 0;

            using var cursor = await sourceCollection.FindAsync(FilterDefinition<JobSeekerProfileSnapshot>.Empty, cancellationToken: token);
            while (await cursor.MoveNextAsync(token))
            {
                foreach (var profile in cursor.Current)
                {
                    token.ThrowIfCancellationRequested();
                    if (string.IsNullOrWhiteSpace(profile.UserId) || existing.Contains(profile.UserId))
                        continue;

                    var projection = MapJobSeeker(profile);
                    if (projection == null)
                        continue;

                    try
                    {
                        await adminContext.JobSeekers.InsertOneAsync(projection, cancellationToken: token);
                        existing.Add(profile.UserId);
                        inserted++;
                    }
                    catch (MongoWriteException ex) when (ex.WriteError?.Code == 11000)
                    {
                        existing.Add(profile.UserId);
                    }
                }
            }

            if (inserted > 0)
            {
                _logger.LogInformation("Admin sync inserted {Count} job seekers.", inserted);
            }
        }

        private async Task SyncJobProvidersAsync(MongoDbContext adminContext, CancellationToken token)
        {
            var settings = _options.Value.JobProvider;
            if (!IsValid(settings))
            {
                _logger.LogWarning("Job provider sync skipped: missing Mongo connection settings.");
                return;
            }

            var sourceCollection = GetSourceCollection<JobProviderProfileSnapshot>(
                settings.ConnectionString,
                settings.DatabaseName,
                JobProviderCollectionName);

            var existingIds = await adminContext.JobProviders
                .Distinct(x => x.JobProviderId, FilterDefinition<AdminJobProviderProjection>.Empty)
                .ToListAsync(token);

            var existing = new HashSet<string>(existingIds);
            var inserted = 0;

            using var cursor = await sourceCollection.FindAsync(FilterDefinition<JobProviderProfileSnapshot>.Empty, cancellationToken: token);
            while (await cursor.MoveNextAsync(token))
            {
                foreach (var profile in cursor.Current)
                {
                    token.ThrowIfCancellationRequested();
                    if (string.IsNullOrWhiteSpace(profile.JobProviderId) || existing.Contains(profile.JobProviderId))
                        continue;

                    var projection = MapJobProvider(profile);
                    if (projection == null)
                        continue;

                    try
                    {
                        await adminContext.JobProviders.InsertOneAsync(projection, cancellationToken: token);
                        existing.Add(profile.JobProviderId);
                        inserted++;
                    }
                    catch (MongoWriteException ex) when (ex.WriteError?.Code == 11000)
                    {
                        existing.Add(profile.JobProviderId);
                    }
                }
            }

            if (inserted > 0)
            {
                _logger.LogInformation("Admin sync inserted {Count} job providers.", inserted);
            }
        }

        private static IMongoCollection<T> GetSourceCollection<T>(string connectionString, string databaseName, string collectionName)
        {
            var client = new MongoClient(connectionString);
            var db = client.GetDatabase(databaseName);
            return db.GetCollection<T>(collectionName);
        }

        private static bool IsValid(SyncMongoSource settings)
        {
            return !string.IsNullOrWhiteSpace(settings.ConnectionString)
                   && !string.IsNullOrWhiteSpace(settings.DatabaseName);
        }

        private static AdminJobSeekerProjection? MapJobSeeker(JobSeekerProfileSnapshot profile)
        {
            if (string.IsNullOrWhiteSpace(profile.UserId))
                return null;

            return new AdminJobSeekerProjection
            {
                UserId = profile.UserId,
                FullName = profile.FullName ?? string.Empty,
                Email = profile.Email ?? string.Empty,
                Phone = profile.Phone,
                Headline = profile.Headline,
                Summary = profile.Summary,
                Skills = profile.Skills ?? new List<string>(),
                ExperienceYears = profile.ExperienceYears,
                Education = profile.Education ?? string.Empty,
                Location = profile.Location,
                WorkHistory = profile.WorkHistory?.Select(item => new AdminWorkExperience
                {
                    Company = item.Company ?? string.Empty,
                    Role = item.Role ?? string.Empty,
                    StartDate = item.StartDate ?? string.Empty,
                    EndDate = item.EndDate ?? string.Empty,
                    Description = item.Description ?? string.Empty,
                    Skills = item.Skills ?? new List<string>()
                }).ToList() ?? new List<AdminWorkExperience>(),
                EducationHistory = profile.EducationHistory?.Select(item => new AdminEducationRecord
                {
                    School = item.School ?? string.Empty,
                    Degree = item.Degree ?? string.Empty,
                    Field = item.Field ?? string.Empty,
                    GraduationYear = item.GraduationYear ?? string.Empty
                }).ToList() ?? new List<AdminEducationRecord>(),
                Projects = profile.Projects?.Select(item => new AdminProjectRecord
                {
                    Name = item.Name ?? string.Empty,
                    Role = item.Role ?? string.Empty,
                    Description = item.Description ?? string.Empty,
                    Link = item.Link
                }).ToList() ?? new List<AdminProjectRecord>(),
                Certifications = profile.Certifications?.Select(item => new AdminCertificationRecord
                {
                    Name = item.Name ?? string.Empty,
                    Issuer = item.Issuer ?? string.Empty,
                    Year = item.Year ?? string.Empty
                }).ToList() ?? new List<AdminCertificationRecord>(),
                Languages = profile.Languages?.Select(item => new AdminLanguageRecord
                {
                    Name = item.Name ?? string.Empty,
                    Proficiency = item.Proficiency ?? string.Empty
                }).ToList() ?? new List<AdminLanguageRecord>(),
                ResumeSettings = new AdminResumeSettings
                {
                    AtsFriendly = profile.ResumeSettings?.AtsFriendly ?? true,
                    Template = profile.ResumeSettings?.Template ?? "Clean"
                },
                CreatedAt = profile.CreatedAt == default ? DateTime.UtcNow : profile.CreatedAt,
                UpdatedAt = profile.UpdatedAt == default ? DateTime.UtcNow : profile.UpdatedAt,
                LastActiveAt = profile.UpdatedAt == default ? DateTime.UtcNow : profile.UpdatedAt,
                IsActive = true
            };
        }

        private static AdminJobProviderProjection? MapJobProvider(JobProviderProfileSnapshot profile)
        {
            if (string.IsNullOrWhiteSpace(profile.JobProviderId))
                return null;

            var updatedAt = profile.UpdatedAt == default ? DateTime.UtcNow : profile.UpdatedAt;
            return new AdminJobProviderProjection
            {
                JobProviderId = profile.JobProviderId,
                CompanyName = profile.CompanyName ?? string.Empty,
                BrandName = profile.BrandName ?? string.Empty,
                Industry = profile.Industry ?? string.Empty,
                CompanySize = profile.CompanySize ?? string.Empty,
                Website = profile.Website ?? string.Empty,
                Phone = profile.Phone ?? string.Empty,
                Location = profile.Location ?? string.Empty,
                About = profile.About ?? string.Empty,
                LogoUrl = profile.LogoUrl,
                LinkedInUrl = profile.LinkedInUrl,
                TwitterUrl = profile.TwitterUrl,
                ContactName = string.Empty,
                ContactEmail = string.Empty,
                CreatedAt = updatedAt,
                UpdatedAt = updatedAt,
                IsActive = true
            };
        }

        private class JobSeekerProfileSnapshot
        {
            [BsonId]
            [BsonRepresentation(BsonType.ObjectId)]
            public string Id { get; set; } = string.Empty;

            public string UserId { get; set; } = string.Empty;
            public string? FullName { get; set; }
            public string? Email { get; set; }
            public string? Phone { get; set; }
            public string? Gender { get; set; }
            public string? Headline { get; set; }
            public string? Summary { get; set; }
            public List<string>? Skills { get; set; }
            public int ExperienceYears { get; set; }
            public string? Education { get; set; }
            public string? Location { get; set; }
            public List<JobSeekerWorkExperience>? WorkHistory { get; set; }
            public List<JobSeekerEducationRecord>? EducationHistory { get; set; }
            public List<JobSeekerProjectRecord>? Projects { get; set; }
            public List<JobSeekerCertificationRecord>? Certifications { get; set; }
            public List<JobSeekerLanguageRecord>? Languages { get; set; }
            public JobSeekerResumeSettings? ResumeSettings { get; set; }
            public DateTime CreatedAt { get; set; }
            public DateTime UpdatedAt { get; set; }
        }

        private class JobSeekerWorkExperience
        {
            public string? Company { get; set; }
            public string? Role { get; set; }
            public string? StartDate { get; set; }
            public string? EndDate { get; set; }
            public string? Description { get; set; }
            public List<string>? Skills { get; set; }
        }

        private class JobSeekerEducationRecord
        {
            public string? School { get; set; }
            public string? Degree { get; set; }
            public string? Field { get; set; }
            public string? GraduationYear { get; set; }
        }

        private class JobSeekerProjectRecord
        {
            public string? Name { get; set; }
            public string? Role { get; set; }
            public string? Description { get; set; }
            public string? Link { get; set; }
        }

        private class JobSeekerCertificationRecord
        {
            public string? Name { get; set; }
            public string? Issuer { get; set; }
            public string? Year { get; set; }
        }

        private class JobSeekerLanguageRecord
        {
            public string? Name { get; set; }
            public string? Proficiency { get; set; }
        }

        private class JobSeekerResumeSettings
        {
            public bool AtsFriendly { get; set; }
            public string Template { get; set; } = "Clean";
        }

        private class JobProviderProfileSnapshot
        {
            [BsonId]
            [BsonRepresentation(BsonType.ObjectId)]
            public string Id { get; set; } = string.Empty;

            public string JobProviderId { get; set; } = string.Empty;
            public string? CompanyName { get; set; }
            public string? BrandName { get; set; }
            public string? Industry { get; set; }
            public string? CompanySize { get; set; }
            public string? Website { get; set; }
            public string? Phone { get; set; }
            public string? Location { get; set; }
            public string? About { get; set; }
            public string? LogoUrl { get; set; }
            public string? LinkedInUrl { get; set; }
            public string? TwitterUrl { get; set; }
            public DateTime UpdatedAt { get; set; }
        }
    }
}
