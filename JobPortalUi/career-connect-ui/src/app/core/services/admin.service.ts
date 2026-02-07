import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from '../../../environments/environment';
import { AdminApplicationProjection } from '../models/admin/admin-application.model';
import { AdminJobProjection } from '../models/admin/admin-job.model';
import { AdminJobProviderProjection } from '../models/admin/admin-job-provider.model';
import { AdminJobSeekerProjection } from '../models/admin/admin-job-seeker.model';
import { AdminSummary } from '../models/admin/admin-summary.model';
import { AdminUser } from '../models/admin/admin-user.model';

@Injectable({ providedIn: 'root' })
export class AdminService {
  private readonly baseUrl = environment.apiUrl;
  private readonly adminBaseUrl = `${this.baseUrl}/admin`;
  private readonly identityAdminBaseUrl = `${this.baseUrl}/identity/admin`;

  constructor(private http: HttpClient) {}

  getSummary() {
    return this.http.get<any>(`${this.identityAdminBaseUrl}/summary`);
  }

  getUsers(params?: { role?: string }) {
    return this.http.get<AdminUser[]>(`${this.identityAdminBaseUrl}/users`, {
      params: params ?? {}
    });
  }

  updateUserStatus(userId: string, isActive: boolean) {
    return this.http.put(`${this.identityAdminBaseUrl}/users/${userId}/status`, { isActive });
  }

  updateUserRole(userId: string, role: string) {
    return this.http.put(`${this.identityAdminBaseUrl}/users/${userId}/role`, { role });
  }

  resetUser(userId: string) {
    return this.http.post(`${this.identityAdminBaseUrl}/users/${userId}/reset`, {});
  }

  getAdminSummary() {
    return this.http.get<AdminSummary>(`${this.adminBaseUrl}/summary`);
  }

  getAdminJobSeekers(params?: { search?: string; city?: string }) {
    return this.http.get<AdminJobSeekerProjection[]>(`${this.adminBaseUrl}/job-seekers`, {
      params: params ?? {}
    });
  }

  getAdminJobSeekerById(userId: string) {
    return this.http.get<AdminJobSeekerProjection>(`${this.adminBaseUrl}/job-seekers/${userId}`);
  }

  getAdminJobProviders(params?: { search?: string; city?: string }) {
    return this.http.get<AdminJobProviderProjection[]>(`${this.adminBaseUrl}/job-providers`, {
      params: params ?? {}
    });
  }

  getAdminJobProviderById(providerId: string) {
    return this.http.get<AdminJobProviderProjection>(`${this.adminBaseUrl}/job-providers/${providerId}`);
  }

  getAdminJobs(params?: { status?: string }) {
    return this.http.get<AdminJobProjection[]>(`${this.adminBaseUrl}/jobs`, {
      params: params ?? {}
    });
  }

  getAdminJobById(jobId: string) {
    return this.http.get<AdminJobProjection>(`${this.adminBaseUrl}/jobs/${jobId}`);
  }

  getAdminApplications(params?: { status?: string }) {
    return this.http.get<AdminApplicationProjection[]>(`${this.adminBaseUrl}/applications`, {
      params: params ?? {}
    });
  }

  getAdminApplicationById(applicationId: string) {
    return this.http.get<AdminApplicationProjection>(`${this.adminBaseUrl}/applications/${applicationId}`);
  }
}
