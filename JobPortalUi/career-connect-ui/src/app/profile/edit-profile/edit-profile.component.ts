import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import {
  FormBuilder,
  FormGroup,
  Validators,
  ReactiveFormsModule
} from '@angular/forms';
import { Router, RouterModule } from '@angular/router';

import { ProfileService } from '../../core/services/profile.service';
import { ProfileResponse } from '../../core/models/profile/profile-response.model';

@Component({
  selector: 'app-edit-profile',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    RouterModule
  ],
  templateUrl: './edit-profile.component.html'
})
export class EditProfileComponent implements OnInit {

  profileForm!: FormGroup;
  isLoading = true;

  constructor(
    private fb: FormBuilder,
    private profileService: ProfileService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.profileService.getProfile().subscribe({
      next: (profile: ProfileResponse) => {
        this.profileForm = this.fb.group({
          fullName: [profile.fullName, Validators.required],
          email: [{ value: profile.email, disabled: true }],
          userType: [{ value: profile.userType, disabled: true }]
        });
        this.isLoading = false;
      }
    });
  }

  onSubmit(): void {
    if (this.profileForm.invalid) return;

    this.profileService.updateProfile({
      fullName: this.profileForm.value.fullName
    }).subscribe({
      next: () => {
        this.router.navigate(['/profile']);
      }
    });
  }

  cancel(): void {
    this.router.navigate(['/profile']);
  }
}
