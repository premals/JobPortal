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
import { ResetPasswordRequest } from '../../core/models/auth/reset-password-request.model';

@Component({
  selector: 'app-reset-password',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,   // ✅ REQUIRED
    RouterModule
  ],
  templateUrl: './reset-password.component.html'
})
export class ResetPasswordComponent implements OnInit {

  // ✅ ALL properties used in HTML
  resetForm!: FormGroup;
  isLoading = false;
  successMessage = '';
  errorMessage = '';

  private email = '';
  private token = '';

  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router
  ) {}

  ngOnInit(): void {
    // 🔑 Read query params
    this.email = this.route.snapshot.queryParamMap.get('email') || '';
    this.token = this.route.snapshot.queryParamMap.get('token') || '';

    // ✅ Initialize form AFTER DI is ready
    this.resetForm = this.fb.group({
      newPassword: ['', [Validators.required, Validators.minLength(6)]],
      confirmPassword: ['', Validators.required]
    });
  }

  onSubmit(): void {
    if (this.resetForm.invalid) {
      this.resetForm.markAllAsTouched();
      return;
    }

    if (
      this.resetForm.value.newPassword !==
      this.resetForm.value.confirmPassword
    ) {
      this.errorMessage = 'Passwords do not match';
      return;
    }

    const payload: ResetPasswordRequest = {
      email: this.email,
      token: this.token,
      newPassword: this.resetForm.value.newPassword
    };

    this.isLoading = true;
    this.errorMessage = '';

    this.authService.resetPassword(payload).subscribe({
      next: () => {
        this.successMessage = 'Password reset successful. Redirecting to login...';
        this.isLoading = false;

        setTimeout(() => {
          this.router.navigate(['/login']);
        }, 1500);
      },
      error: () => {
        this.errorMessage = 'Invalid or expired reset link';
        this.isLoading = false;
      }
    });
  }
}
