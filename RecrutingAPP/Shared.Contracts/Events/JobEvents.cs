using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Contracts.Events
{
    public class JobEvents
    {
        public record JobCreatedEvent
        {
            // ============================
            // Event Metadata
            // ============================
            public string EventId { get; init; } = Guid.NewGuid().ToString();
            public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
            public string EventType { get; init; } = nameof(JobCreatedEvent);
            public int Version { get; init; } = 1;

            // ============================
            // Job Identity
            // ============================
            public string JobId { get; init; } = null!;
            public string JobProviderId { get; init; } = null!;

            // ============================
            // Basic Job Info
            // ============================
            public string Title { get; init; } = null!;
            public string Description { get; init; } = null!;
            public string EmploymentType { get; init; } = null!;
            public string WorkMode { get; init; } = null!;

            // ============================
            // Experience
            // ============================
            public int MinExperience { get; init; }
            public int MaxExperience { get; init; }

            // ============================
            // Location
            // ============================
            public string City { get; init; } = null!;
            public string State { get; init; } = null!;
            public string Country { get; init; } = null!;

            // ============================
            // Salary
            // ============================
            public decimal MinSalary { get; init; }
            public decimal MaxSalary { get; init; }
            public string Currency { get; init; } = "INR";
            public string SalaryFrequency { get; init; } = "Yearly";

            // ============================
            // Skills & Education
            // ============================
            public List<string> KeySkills { get; init; } = new();
            public string Education { get; init; } = null!;
            public string Industry { get; init; } = null!;

            // ============================
            // Job Meta
            // ============================
            public int Openings { get; init; }
            public DateTime PostedAt { get; init; }
            public DateTime? ExpiryDate { get; init; }
            public string Status { get; init; } = "Active";
        }

        /// <summary>
        /// Published when a job is partially updated by the Job Provider.
        /// Only changed fields are populated (nullable properties).
        /// </summary>
        public record JobUpdatedEvent
        {
            // ============================
            // Event Metadata
            // ============================
            public string EventId { get; init; } = Guid.NewGuid().ToString();
            public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
            public string EventType { get; init; } = nameof(JobUpdatedEvent);
            public int Version { get; init; } = 1;

            // ============================
            // Identity
            // ============================
            public string JobId { get; init; } = null!;
            public string JobProviderId { get; init; } = null!;

            // ============================
            // Updated Fields (nullable = changed)
            // ============================

            // Basic Job Info
            public string? Title { get; init; }
            public string? Description { get; init; }
            public string? EmploymentType { get; init; }
            public string? WorkMode { get; init; }

            // Experience
            public int? MinExperience { get; init; }
            public int? MaxExperience { get; init; }

            // Location
            public string? City { get; init; }
            public string? State { get; init; }
            public string? Country { get; init; }

            // Salary
            public decimal? MinSalary { get; init; }
            public decimal? MaxSalary { get; init; }
            public string? Currency { get; init; }
            public string? SalaryFrequency { get; init; }

            // Skills & Education
            public List<string>? KeySkills { get; init; }
            public string? Education { get; init; }
            public string? Industry { get; init; }

            // Job Meta
            public int? Openings { get; init; }
            public DateTime? ExpiryDate { get; init; }
        }


        /// <summary>
        /// Published when a job is fully updated (PUT semantics).
        /// Represents a complete job snapshot.
        /// </summary>
        public record JobFullyUpdatedEvent
        {
            // ============================
            // Event Metadata
            // ============================
            public string EventId { get; init; } = Guid.NewGuid().ToString();
            public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
            public string EventType { get; init; } = nameof(JobFullyUpdatedEvent);
            public int Version { get; init; } = 1;

            // ============================
            // Identity
            // ============================
            public string JobId { get; init; } = null!;
            public string JobProviderId { get; init; } = null!;

            // ============================
            // Full Job Snapshot
            // ============================
            public string Title { get; init; } = null!;
            public string Description { get; init; } = null!;
            public string EmploymentType { get; init; } = null!;
            public string WorkMode { get; init; } = null!;

            public int MinExperience { get; init; }
            public int MaxExperience { get; init; }

            public string City { get; init; } = null!;
            public string State { get; init; } = null!;
            public string Country { get; init; } = null!;

            public decimal MinSalary { get; init; }
            public decimal MaxSalary { get; init; }
            public string Currency { get; init; } = "INR";
            public string SalaryFrequency { get; init; } = "Yearly";

            public List<string> KeySkills { get; init; } = new();
            public string Education { get; init; } = null!;
            public string Industry { get; init; } = null!;

            public int Openings { get; init; }
            public DateTime PostedAt { get; init; }
            public DateTime? ExpiryDate { get; init; }

            public string Status { get; init; } = "Active";
        }

        /// <summary>
        /// Published when a job is closed and should no longer be visible to job seekers.
        /// </summary>
        public record JobClosedEvent
        {
            // Event metadata
            public string EventId { get; init; } = Guid.NewGuid().ToString();
            public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
            public string EventType { get; init; } = nameof(JobClosedEvent);
            public int Version { get; init; } = 1;

            // Business data
            public string JobId { get; init; } = null!;
            public string JobProviderId { get; init; } = null!;
            public DateTime ClosedAt { get; init; } = DateTime.UtcNow;
        }


        public record JobAppliedEvent : BaseEvent
        {
            // =====================
            // Event Metadata
            // =====================
            public string EventId { get; init; } = Guid.NewGuid().ToString();
            public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
            public string EventType { get; init; } = nameof(JobAppliedEvent);
            public int Version { get; init; } = 1;

            // =====================
            // Job Info
            // =====================
            public string JobId { get; init; } = null!;
            public string JobProviderId { get; init; } = null!;

            // =====================
            // Job Seeker Info
            // =====================
            public string JobSeekerId { get; init; } = null!;
            public string FullName { get; init; } = null!;
            public string Email { get; init; } = null!;
            public string Phone { get; init; } = null!;

            // =====================
            // Application Info
            // =====================
            public string ResumeUrl { get; init; } = null!;

            public DateTime AppliedAt { get; init; } = DateTime.UtcNow;
            public string Status { get; init; } = null!;
        }

        public record JobApplicationWithdrawnEvent : BaseEvent
        {
            // ============================
            // Event Metadata
            // ============================
            public string EventId { get; init; } = Guid.NewGuid().ToString();
            public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
            public string EventType { get; init; } = nameof(JobApplicationWithdrawnEvent);
            public int Version { get; init; } = 1;

            // ============================
            // Identity
            // ============================
            public string JobId { get; init; } = null!;
            public string JobProviderId { get; init; } = null!;
            public string JobSeekerId { get; init; } = null!;
        }

        public record JobApplicationStatusUpdatedEvent : BaseEvent
        {
            // ============================
            // Event Metadata
            // ============================
            public string EventId { get; init; } = Guid.NewGuid().ToString();
            public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
            public string EventType { get; init; } = nameof(JobApplicationStatusUpdatedEvent);
            public int Version { get; init; } = 1;

            // ============================
            // Identity
            // ============================
            public string JobId { get; init; } = null!;
            public string JobProviderId { get; init; } = null!;
            public string JobSeekerId { get; init; } = null!;

            // ============================
            // Status
            // ============================
            public string Status { get; init; } = null!;
            public DateTime UpdatedAt { get; init; } = DateTime.UtcNow;
        }

        public record InterviewInviteCreatedEvent : BaseEvent
        {
            public string EventId { get; init; } = Guid.NewGuid().ToString();
            public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
            public string EventType { get; init; } = nameof(InterviewInviteCreatedEvent);
            public int Version { get; init; } = 1;

            public string InviteId { get; init; } = null!;
            public string JobId { get; init; } = null!;
            public string JobTitle { get; init; } = null!;
            public string JobProviderId { get; init; } = null!;
            public string JobSeekerId { get; init; } = null!;

            public string CandidateName { get; init; } = null!;
            public string CandidateEmail { get; init; } = null!;

            public string Difficulty { get; init; } = "Medium";
            public List<DateTime> ProposedSlots { get; init; } = new();
        }

        public record JobSeekerRegisteredEvent
        {
            // ============================
            // Event Metadata
            // ============================
            public string EventId { get; init; } = Guid.NewGuid().ToString();
            public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
            public string EventType { get; init; } = nameof(JobSeekerRegisteredEvent);
            public int Version { get; init; } = 1;

            // ============================
            // Identity Info
            // ============================
            public string UserId { get; init; } = null!;
            public string FullName { get; init; } = null!;
            public string Email { get; init; } = null!;
        }


        public record JobProviderRegisteredEvent
        {
            // ============================
            // Event Metadata
            // ============================
            public string EventId { get; init; } = Guid.NewGuid().ToString();
            public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
            public string EventType { get; init; } = nameof(JobProviderRegisteredEvent);
            public int Version { get; init; } = 1;

            // ============================
            // Identity Info
            // ============================
            public string UserId { get; init; } = null!;
            public string FullName { get; init; } = null!;
            public string Email { get; init; } = null!;
        }

        public record JobSeekerProfileUpsertedEvent
        {
            // ============================
            // Event Metadata
            // ============================
            public string EventId { get; init; } = Guid.NewGuid().ToString();
            public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
            public string EventType { get; init; } = nameof(JobSeekerProfileUpsertedEvent);
            public int Version { get; init; } = 1;

            // ============================
            // Identity Info
            // ============================
            public string UserId { get; init; } = null!;
            public string FullName { get; init; } = null!;
            public string Email { get; init; } = null!;

            // ============================
            // Profile Info
            // ============================
            public string? Phone { get; init; }
            public string? Headline { get; init; }
            public string? Summary { get; init; }
            public List<string> Skills { get; init; } = new();
            public int ExperienceYears { get; init; }
            public string Education { get; init; } = string.Empty;
            public string? Location { get; init; }
            public List<JobSeekerWorkHistoryItem> WorkHistory { get; init; } = new();
            public List<JobSeekerEducationRecordItem> EducationHistory { get; init; } = new();
            public List<JobSeekerProjectRecordItem> Projects { get; init; } = new();
            public List<JobSeekerCertificationRecordItem> Certifications { get; init; } = new();
            public List<JobSeekerLanguageRecordItem> Languages { get; init; } = new();
            public JobSeekerResumeSettings ResumeSettings { get; init; } = new();

            public DateTime CreatedAt { get; init; }
            public DateTime UpdatedAt { get; init; }
        }

        public record JobSeekerWorkHistoryItem
        {
            public string Company { get; init; } = string.Empty;
            public string Role { get; init; } = string.Empty;
            public string StartDate { get; init; } = string.Empty;
            public string EndDate { get; init; } = string.Empty;
            public string Description { get; init; } = string.Empty;
            public List<string> Skills { get; init; } = new();
        }

        public record JobSeekerEducationRecordItem
        {
            public string School { get; init; } = string.Empty;
            public string Degree { get; init; } = string.Empty;
            public string Field { get; init; } = string.Empty;
            public string GraduationYear { get; init; } = string.Empty;
        }

        public record JobSeekerProjectRecordItem
        {
            public string Name { get; init; } = string.Empty;
            public string Role { get; init; } = string.Empty;
            public string Description { get; init; } = string.Empty;
            public string? Link { get; init; }
        }

        public record JobSeekerCertificationRecordItem
        {
            public string Name { get; init; } = string.Empty;
            public string Issuer { get; init; } = string.Empty;
            public string Year { get; init; } = string.Empty;
        }

        public record JobSeekerLanguageRecordItem
        {
            public string Name { get; init; } = string.Empty;
            public string Proficiency { get; init; } = string.Empty;
        }

        public record JobSeekerResumeSettings
        {
            public bool AtsFriendly { get; init; } = true;
            public string Template { get; init; } = "Clean";
        }

        public record JobProviderProfileUpsertedEvent
        {
            // ============================
            // Event Metadata
            // ============================
            public string EventId { get; init; } = Guid.NewGuid().ToString();
            public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
            public string EventType { get; init; } = nameof(JobProviderProfileUpsertedEvent);
            public int Version { get; init; } = 1;

            // ============================
            // Identity
            // ============================
            public string JobProviderId { get; init; } = null!;

            // ============================
            // Profile
            // ============================
            public string CompanyName { get; init; } = string.Empty;
            public string BrandName { get; init; } = string.Empty;
            public string Industry { get; init; } = string.Empty;
            public string CompanySize { get; init; } = string.Empty;
            public string Website { get; init; } = string.Empty;
            public string Phone { get; init; } = string.Empty;
            public string Location { get; init; } = string.Empty;
            public string About { get; init; } = string.Empty;
            public string? LogoUrl { get; init; }
            public string? LinkedInUrl { get; init; }
            public string? TwitterUrl { get; init; }

            public string ContactName { get; init; } = string.Empty;
            public string ContactEmail { get; init; } = string.Empty;

            public DateTime CreatedAt { get; init; }
            public DateTime UpdatedAt { get; init; }
        }
        public abstract record BaseEvent
        {
            public string EventId { get; init; } = Guid.NewGuid().ToString();
            public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
            public string EventType { get; init; } = null!;
            public int Version { get; init; } = 1;
        }

    }
}

