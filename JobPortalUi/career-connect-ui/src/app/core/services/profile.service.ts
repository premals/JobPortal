import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

// Profile models
import { ProfileResponse } from '../models/profile/profile-response.model';

@Injectable({ providedIn: 'root' })
export class ProfileService {

  private readonly baseUrl = environment.apiUrl;

  constructor(private http: HttpClient) {}

  /**
   * GET /api/Profile/me
   */
  getProfile(): Observable<ProfileResponse> {
    return this.http.get<ProfileResponse>(
      `${this.baseUrl}/Profile/me`
    );
  }

  /**
   * PUT /api/Profile/update
   * (optional – add when needed)
   */
  updateProfile(payload: Partial<ProfileResponse>): Observable<void> {
    return this.http.put<void>(
      `${this.baseUrl}/Profile/update`,
      payload
    );
  }
}