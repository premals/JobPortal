import { AdminApplicationProjection } from '../core/models/admin/admin-application.model';
import { AdminJobProjection } from '../core/models/admin/admin-job.model';
import { AdminJobProviderProjection } from '../core/models/admin/admin-job-provider.model';
import { AdminJobSeekerProjection } from '../core/models/admin/admin-job-seeker.model';
import {
  AdminHiringApplication,
  AdminJobProvider,
  AdminJobSeeker,
  AdminModerationJob
} from './admin-data';

const toIsoDate = (value?: string | Date | null): string => {
  if (!value) return new Date().toISOString();
  const parsed = new Date(value);
  return Number.isNaN(parsed.getTime()) ? new Date().toISOString() : parsed.toISOString();
};

export const mapJobSeekerProjection = (item: AdminJobSeekerProjection): AdminJobSeeker => ({
  id: item.UserId,
  fullName: item.FullName,
  email: item.Email,
  city: item.Location ?? 'Unknown',
  status: item.IsActive ? 'Active' : 'Blocked',
  createdAt: toIsoDate(item.CreatedAt),
  lastActiveAt: toIsoDate(item.LastActiveAt ?? item.UpdatedAt ?? item.CreatedAt),
  verifiedEmail: false,
  verifiedPhone: false,
  phone: item.Phone ?? undefined,
  headline: item.Headline ?? undefined,
  skills: item.Skills ?? [],
  notes: []
});

export const mapJobProviderProjection = (item: AdminJobProviderProjection): AdminJobProvider => ({
  id: item.JobProviderId,
  companyName: item.CompanyName,
  contactName: item.ContactName,
  email: item.ContactEmail,
  city: item.Location ?? 'Unknown',
  status: item.IsActive ? 'Active' : 'Blocked',
  createdAt: toIsoDate(item.CreatedAt),
  postedJobs: [],
  hires: 0,
  complaints: 0,
  notes: []
});

export const mapJobProjection = (
  job: AdminJobProjection,
  providerLookup?: Map<string, AdminJobProviderProjection>
): AdminModerationJob => {
  const providerName = providerLookup?.get(job.JobProviderId)?.CompanyName;
  const cityParts = [job.City, job.State, job.Country].filter(Boolean);
  const city = cityParts.length > 0 ? cityParts.join(', ') : job.City || 'Unknown';
  const status = (job.Status || 'Active') as AdminModerationJob['status'];

  return {
    id: job.JobId,
    title: job.Title,
    provider: providerName ?? job.JobProviderId,
    city,
    status,
    postedAt: toIsoDate(job.PostedAt),
    expiresAt: job.ExpiryDate ? toIsoDate(job.ExpiryDate) : '',
    reports: 0,
    flaggedAsScam: status.toLowerCase() === 'scam',
    hidden: status.toLowerCase() === 'hidden'
  };
};

export const mapApplicationProjection = (
  application: AdminApplicationProjection,
  jobLookup: Map<string, AdminJobProjection>,
  providerLookup: Map<string, AdminJobProviderProjection>
): AdminHiringApplication => {
  const job = jobLookup.get(application.JobId);
  const provider = providerLookup.get(application.JobProviderId);
  const cityParts = job ? [job.City, job.State, job.Country].filter(Boolean) : [];
  const city = cityParts.length > 0 ? cityParts.join(', ') : job?.City ?? 'Unknown';

  return {
    id: application.ApplicationId,
    candidateId: application.JobSeekerId,
    candidateName: application.CandidateName || application.CandidateEmail || 'Candidate',
    jobTitle: job?.Title ?? application.JobId,
    jobProvider: provider?.CompanyName ?? application.JobProviderId,
    city,
    stage: application.Status,
    recruiter: 'Unassigned',
    appliedAt: toIsoDate(application.AppliedAt),
    lastUpdated: toIsoDate(application.UpdatedAt),
    timeline: [],
    interviewNotes: [],
    internalNotes: []
  };
};
