export interface AdminApplicationProjection {
  ApplicationId: string;
  JobId: string;
  JobProviderId: string;
  JobSeekerId: string;
  CandidateName: string;
  CandidateEmail: string;
  CandidatePhone?: string;
  ResumeUrl?: string;
  Status: string;
  AppliedAt: string;
  UpdatedAt: string;
}
