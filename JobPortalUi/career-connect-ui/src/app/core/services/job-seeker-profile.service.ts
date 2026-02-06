import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from '../../../environments/environment';
import { JobSeekerProfile } from '../models/job-seeker/job-seeker-profile.model';

@Injectable({ providedIn: 'root' })
export class JobSeekerProfileService {
  private readonly baseUrl = environment.apiUrl;

  constructor(private http: HttpClient) {}

  getProfile() {
    return this.http.get<JobSeekerProfile>(`${this.baseUrl}/jobseeker/profile`);
  }

  updateProfile(payload: JobSeekerProfile) {
    return this.http.put<JobSeekerProfile>(`${this.baseUrl}/jobseeker/profile`, payload);
  }

  generateResumeAi(payload: any) {
    return this.http.post<string>(`${this.baseUrl}/jobseeker/resume/ai-generate`, payload);
  }
}
