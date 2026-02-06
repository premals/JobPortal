import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable, tap } from 'rxjs';
import { environment } from '../../../environments/environment';

// Auth models
import { RegisterRequest } from '../models/auth/register-request.model';
import { LoginRequest } from '../models/auth/login-request.model';
import { AuthResponse } from '../models/auth/auth-response.model';
import { ForgotPasswordRequest } from '../models/auth/forgot-password-request.model';
import { ResetPasswordRequest } from '../models/auth/reset-password-request.model';
import { Router } from '@angular/router';

@Injectable({ providedIn: 'root' })
export class AuthService {

  private readonly baseUrl = environment.apiUrl;

  constructor(private http: HttpClient,
    private router: Router
  ) {}

  private canUseStorage(): boolean {
    return typeof window !== 'undefined' && typeof localStorage !== 'undefined';
  }

  refreshToken(): Observable<any> {
  const refreshToken = this.canUseStorage()
    ? localStorage.getItem('refreshToken')
    : null;

  return this.http.post<any>(
    `${this.baseUrl}/identity/refresh-token`,
    { refreshToken }
  ).pipe(
    tap(res => {
      // Update tokens
      if (this.canUseStorage()) {
        localStorage.setItem('accessToken', res.accessToken);
        localStorage.setItem('refreshToken', res.refreshToken);

        localStorage.setItem('userType', res.profile.userType);
        localStorage.setItem('userId', res.profile.userId);
        localStorage.setItem('email', res.profile.email);
      }

    })
  );
}

/**
 * Logout user
 */
logout(): void {
  if (this.canUseStorage()) {
    localStorage.clear();
  }
  this.router.navigate(['/login']);
}

  /**
   * POST /api/Auth/register
   */
  register(payload: RegisterRequest): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/identity/register`, payload);
  }

  /**
   * POST /api/Auth/login
   */
  login(payload: LoginRequest): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.baseUrl}/identity/login`, payload);
  }

  /**
   * POST /api/Auth/forgot-password
   */
  forgotPassword(email: string): Observable<void> {
    const payload: ForgotPasswordRequest = { email };
    return this.http.post<void>(
      `${this.baseUrl}/identity/forgot-password`,
      payload
    );
  }

  /**
   * POST /api/Auth/reset-password
   */
  resetPassword(payload: ResetPasswordRequest): Observable<void> {
    return this.http.post<void>(
      `${this.baseUrl}/identity/reset-password`,
      payload
    );
  }
}
