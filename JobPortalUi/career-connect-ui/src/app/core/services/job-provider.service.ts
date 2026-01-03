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
}
