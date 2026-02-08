import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from '../../../environments/environment';
import { JobSeekerProfile } from '../models/job-seeker/job-seeker-profile.model';
import { ResumeParseResult } from '../models/job-seeker/resume-parse-result.model';
import { ResumeDraft, ResumeDraftRequest } from '../models/job-seeker/resume-draft.model';

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

  parseResume(text: string) {
    return this.http.post<ResumeParseResult>(
      `${this.baseUrl}/jobseeker/resume/parse`,
      { text }
    );
  }

  parseResumeFile(file: File) {
    const formData = new FormData();
    formData.append('file', file, file.name);
    return this.http.post<ResumeParseResult>(
      `${this.baseUrl}/jobseeker/resume/parse-file`,
      formData
    );
  }

  getResumeDraft() {
    return this.http.get<ResumeDraft>(`${this.baseUrl}/jobseeker/resume/draft`);
  }

  saveResumeDraft(payload: ResumeDraftRequest) {
    return this.http.put<ResumeDraft>(`${this.baseUrl}/jobseeker/resume/draft`, payload);
  }

  generateResumePdf(payload?: ResumeDraftRequest) {
    return this.http.post(`${this.baseUrl}/jobseeker/resume/pdf`, payload ?? null, {
      responseType: 'blob'
    });
  }
}
