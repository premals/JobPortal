import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import {
  FormBuilder,
  FormGroup,
  Validators,
  ReactiveFormsModule
} from '@angular/forms';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';

import { AuthService } from '../../core/services/auth.service';
import { ToastService } from '../../core/services/toast.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    RouterModule
  ],
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.scss']
})
export class LoginComponent implements OnInit {

  loginForm!: FormGroup;
  isLoading = false;
  errorMessage = '';
  showPassword = false;

  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private router: Router,
    private route: ActivatedRoute,
    private toastService: ToastService
  ) {
    // ✅ Initialize form inside constructor (Angular 18 safe)
    this.loginForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      password: ['', Validators.required]
    });
  }

  ngOnInit(): void {
    const verified = this.route.snapshot.queryParamMap.get('verified');
    const message = this.route.snapshot.queryParamMap.get('message');
    const shouldShowToast = verified === '1' || verified === 'true' || !!message;

    if (shouldShowToast) {
      this.toastService.show(message ?? 'Your email id verified successfully');
      this.router.navigate([], {
        queryParams: { verified: null, message: null },
        queryParamsHandling: 'merge',
        replaceUrl: true
      });
    }
  }

  onSubmit(): void {
    if (this.loginForm.invalid) {
      this.loginForm.markAllAsTouched();
      return;
    }

    this.isLoading = true;
    this.errorMessage = '';

    this.authService.login(this.loginForm.value).subscribe({
      next: (res: any) => {

        localStorage.setItem('accessToken', res.accessToken);
        localStorage.setItem('refreshToken', res.refreshToken);

        // ✅ Store user profile info
        localStorage.setItem('userType', res.profile.userType);
        localStorage.setItem('userId', res.profile.userId);
        localStorage.setItem('email', res.profile.email);

        // ✅ Role-based redirect
        if (res.profile.userType === 'JobProvider') {
          this.router.navigate(['/job-provider']);
        }
        else if (res.profile.userType === 'JobSeeker') {
          this.router.navigate(['/job-seeker']);
        }
        else if (res.profile.userType === 'Admin') {
          this.router.navigate(['/admin']);
        }
        else {
          this.router.navigate(['/profile']);
        }
      },
      error: (err) => {
        const backendMessage = typeof err?.error === 'string'
          ? err.error
          : err?.error?.message ?? err?.error?.detail;
        this.errorMessage = backendMessage || 'Invalid email or password';
        this.isLoading = false;
      }
    });
  }

  togglePassword(): void {
    this.showPassword = !this.showPassword;
  }

  isFieldInvalid(controlName: string): boolean {
    const control = this.loginForm.get(controlName);
    return !!control && control.invalid && (control.dirty || control.touched);
  }

  get emailError(): string | null {
    const control = this.loginForm.get('email');
    if (!control || !this.isFieldInvalid('email')) {
      return null;
    }

    if (control.hasError('required')) {
      return 'Email is required';
    }

    if (control.hasError('email')) {
      return 'Enter a valid email address';
    }

    return null;
  }

  get passwordError(): string | null {
    const control = this.loginForm.get('password');
    if (!control || !this.isFieldInvalid('password')) {
      return null;
    }

    if (control.hasError('required')) {
      return 'Password is required';
    }

    return null;
  }
}
