import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

// Auth models
import { RegisterRequest } from '../models/auth/register-request.model';
import { LoginRequest } from '../models/auth/login-request.model';
import { AuthResponse } from '../models/auth/auth-response.model';
import { ForgotPasswordRequest } from '../models/auth/forgot-password-request.model';
import { ResetPasswordRequest } from '../models/auth/reset-password-request.model';

@Injectable({ providedIn: 'root' })
export class AuthService {

  private readonly baseUrl = environment.apiUrl;

  constructor(private http: HttpClient) {}

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