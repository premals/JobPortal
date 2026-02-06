import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from '../../../environments/environment';
import { AdminUser } from '../models/admin/admin-user.model';

@Injectable({ providedIn: 'root' })
export class AdminService {
  private readonly baseUrl = environment.apiUrl;

  constructor(private http: HttpClient) {}

  getSummary() {
    return this.http.get<any>(`${this.baseUrl}/identity/admin/summary`);
  }

  getUsers(params?: { role?: string }) {
    return this.http.get<AdminUser[]>(`${this.baseUrl}/identity/admin/users`, {
      params: params ?? {}
    });
  }

  updateUserStatus(userId: string, isActive: boolean) {
    return this.http.put(`${this.baseUrl}/identity/admin/users/${userId}/status`, { isActive });
  }

  updateUserRole(userId: string, role: string) {
    return this.http.put(`${this.baseUrl}/identity/admin/users/${userId}/role`, { role });
  }

  resetUser(userId: string) {
    return this.http.post(`${this.baseUrl}/identity/admin/users/${userId}/reset`, {});
  }
}
