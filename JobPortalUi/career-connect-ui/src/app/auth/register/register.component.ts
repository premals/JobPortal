import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import {
  FormBuilder,
  FormGroup,
  Validators,
  ReactiveFormsModule
} from '@angular/forms';
import { Router, RouterModule } from '@angular/router';

import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    RouterModule
  ],
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.scss']
})
export class RegisterComponent {

  registerForm!: FormGroup;
  isLoading = false;
  errorMessage = '';
  showPassword = false;

  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private router: Router
  ) {
    // Initialize form safely (Angular 18)
    this.registerForm = this.fb.group({
      fullName: [''],
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required, Validators.minLength(6)]],
      userType: ['', Validators.required]
    });
  }

  onSubmit(): void {
    if (this.registerForm.invalid) {
      this.registerForm.markAllAsTouched();
      return;
    }

    this.isLoading = true;
    this.errorMessage = '';

    this.authService.register(this.registerForm.value).subscribe({
      next: () => {
        this.isLoading = false;

        // ✅ Redirect to Login after successful registration
        this.router.navigate(['/login']);
      },
      error: (err) => {
        this.isLoading = false;
        const backendMessage = typeof err?.error === 'string'
          ? err.error
          : err?.error?.message ?? err?.error?.detail;
        this.errorMessage = backendMessage || 'Registration failed';
      }
    });
  }

  togglePassword(): void {
    this.showPassword = !this.showPassword;
  }

  selectRole(role: string): void {
    this.registerForm.patchValue({ userType: role });
  }

  isFieldInvalid(controlName: string): boolean {
    const control = this.registerForm.get(controlName);
    return !!control && control.invalid && (control.dirty || control.touched);
  }

  get emailError(): string | null {
    const control = this.registerForm.get('email');
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
    const control = this.registerForm.get('password');
    if (!control || !this.isFieldInvalid('password')) {
      return null;
    }

    if (control.hasError('required')) {
      return 'Password is required';
    }

    if (control.hasError('minlength')) {
      return 'Password must be at least 6 characters';
    }

    return null;
  }

  get roleError(): string | null {
    const control = this.registerForm.get('userType');
    if (!control || !this.isFieldInvalid('userType')) {
      return null;
    }

    if (control.hasError('required')) {
      return 'Please select a role to continue';
    }

    return null;
  }
}
