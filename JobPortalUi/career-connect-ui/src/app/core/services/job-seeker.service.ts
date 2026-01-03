import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from '../../../environments/environment';
import { ApplyJobRequest } from '../models/job-seeker/apply-job-request.model';
import { ResumeAiRequest } from '../models/job-seeker/resume-ai-request.model';

@Injectable({ providedIn: 'root' })
export class JobSeekerService {

  private baseUrl = environment.apiUrl;

  constructor(private http: HttpClient) {}

  // ---------------- JOB BROWSE ----------------

  getJobs(page = 1, pageSize = 20) {
    return this.http.get(
      `${this.baseUrl}/jobs?page=${page}&pageSize=${pageSize}`
    );
  }

  getJobById(jobId: string) {
    return this.http.get(`${this.baseUrl}/jobs/${jobId}`);
  }

  searchJobs(params: any) {
    return this.http.get(`${this.baseUrl}/jobs/search`, { params });
  }

  getJobsBySkill(skill: string) {
    return this.http.get(`${this.baseUrl}/jobs/by-skill?skill=${skill}`);
  }

  getJobsByLocation(city: string) {
    return this.http.get(`${this.baseUrl}/jobs/by-location?city=${city}`);
  }

  getSimilarJobs(jobId: string) {
    return this.http.get(`${this.baseUrl}/jobs/similar/${jobId}`);
  }

  getRecommendedJobs() {
    return this.http.get(`${this.baseUrl}/jobs/recommended`);
  }

  // ---------------- APPLICATIONS ----------------

  applyJob(jobId: string) {
    const payload: ApplyJobRequest = { jobId };
    return this.http.post(
      `${this.baseUrl}/jobseeker/applications/apply`,
      payload
    );
  }

  getMyApplications() {
    return this.http.get(`${this.baseUrl}/jobseeker/applications`);
  }

  withdrawApplication(jobId: string) {
    return this.http.post(
      `${this.baseUrl}/jobseeker/applications/${jobId}/withdraw`,
      {}
    );
  }

  getApplicationStatus(jobId: string) {
    return this.http.get(
      `${this.baseUrl}/jobseeker/applications/${jobId}`
    );
  }

  // ---------------- RESUME AI ----------------

  generateResume(payload: ResumeAiRequest) {
    return this.http.post(
      `${this.baseUrl}/jobseeker/resume/ai-generate`,
      payload
    );
  }
}
