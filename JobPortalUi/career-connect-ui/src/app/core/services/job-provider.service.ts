import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from '../../../environments/environment';

@Injectable({ providedIn: 'root' })
export class JobProviderService {

  private baseUrl = environment.apiUrl;

  constructor(private http: HttpClient) {}

  createJob(payload: any) {
    return this.http.post(`${this.baseUrl}/jobprovider/jobs`, payload);
  }

  getMyJobs() {
    return this.http.get(`${this.baseUrl}/jobprovider/jobs/my`);
  }

  getApplicationsByJob(jobId: string) {
    return this.http.get<any[]>(
      `${this.baseUrl}/jobprovider/jobs/applications/${jobId}`
    );
  }

  getApplicationsSummary(limit = 6) {
    return this.http.get<any>(
      `${this.baseUrl}/jobprovider/jobs/applications/summary`,
      { params: { limit } }
    );
  }

  updateApplicationStatus(jobId: string, jobSeekerId: string, status: string) {
    return this.http.patch(
      `${this.baseUrl}/jobprovider/jobs/${jobId}/applications/${jobSeekerId}/status`,
      {},
      { params: { status } }
    );
  }

  getAiShortlistSuggestion(jobId: string, jobSeekerId: string) {
    return this.http.post<any>(
      `${this.baseUrl}/jobprovider/jobs/${jobId}/applications/${jobSeekerId}/ai-suggest`,
      {}
    );
  }

  sendInterviewInvite(jobId: string, jobSeekerId: string, payload: any) {
    return this.http.post<any>(
      `${this.baseUrl}/jobprovider/jobs/${jobId}/applications/${jobSeekerId}/invite`,
      payload
    );
  }

  getInterviewReport(jobId: string, jobSeekerId: string) {
    return this.http.get<any>(
      `${this.baseUrl}/jobprovider/jobs/interviews/report`,
      { params: { jobId, jobSeekerId } }
    );
  }

  getScheduledInvites(from?: string, to?: string) {
    const params: any = {};
    if (from) params.from = from;
    if (to) params.to = to;
    return this.http.get<any[]>(
      `${this.baseUrl}/jobprovider/jobs/interviews/provider/invites`,
      { params }
    );
  }

  getJobProviderSettings() {
    return this.http.get<any>(`${this.baseUrl}/jobprovider/jobs/settings`);
  }

  updateJobProviderSettings(payload: any) {
    return this.http.put<any>(`${this.baseUrl}/jobprovider/jobs/settings`, payload);
  }

  getJobProviderProfile() {
    return this.http.get<any>(`${this.baseUrl}/jobprovider/jobs/provider-profile`);
  }

  updateJobProviderProfile(payload: any) {
    return this.http.put<any>(`${this.baseUrl}/jobprovider/jobs/provider-profile`, payload);
  }

  updateJob(jobId: string, payload: any) {
    return this.http.put(`${this.baseUrl}/jobprovider/jobs/${jobId}`, payload);
  }

  getJobById(jobId: string) {
    return this.http.get<any>(`${this.baseUrl}/jobprovider/jobs/${jobId}`);
  }
}
