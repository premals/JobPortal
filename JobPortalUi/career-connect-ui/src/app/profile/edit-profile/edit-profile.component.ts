import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { ProfileService } from '../../core/services/profile.service';
import { JobProviderService } from '../../core/services/job-provider.service';
import { JobSeekerProfileService } from '../../core/services/job-seeker-profile.service';
import { ProfileResponse } from '../../core/models/profile/profile-response.model';
import { JobProviderProfile } from '../../core/models/job-provider/job-provider-profile.model';
import { JobSeekerProfile } from '../../core/models/job-seeker/job-seeker-profile.model';

@Component({
  selector: 'app-edit-profile',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './edit-profile.component.html',
  styleUrls: ['./edit-profile.component.scss']
})
export class EditProfileComponent implements OnInit {
  @Input() embedded = false;
  @Output() cancelled = new EventEmitter<void>();
  userType = '';
  isLoading = true;
  isSaving = false;
  successMessage = '';
  errorMessage = '';
  jobSeekerSubmitAttempted = false;

  profile: ProfileResponse = { fullName: '', email: '' };
  jobProviderProfile: JobProviderProfile = {
    companyName: '',
    brandName: '',
    industry: '',
    companySize: '',
    website: '',
    phone: '',
    location: '',
    about: '',
    logoUrl: '',
    linkedInUrl: '',
    twitterUrl: ''
  };

  jobSeekerProfile: JobSeekerProfile = {
    fullName: '',
    email: '',
    gender: '',
    skills: [],
    experienceYears: 0,
    education: '',
    workHistory: [],
    educationHistory: [],
    projects: [],
    certifications: [],
    languages: [],
    resumeSettings: {
      atsFriendly: true,
      template: 'Clean'
    }
  };

  constructor(
    private profileService: ProfileService,
    private jobProviderService: JobProviderService,
    private jobSeekerService: JobSeekerProfileService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.userType = localStorage.getItem('userType') ?? '';

    if (this.userType === 'JobProvider') {
      this.loadJobProviderProfile();
      return;
    }

    if (this.userType === 'JobSeeker') {
      this.loadJobSeekerProfile();
      return;
    }

    this.profileService.getProfile().subscribe({
      next: (res) => {
        this.profile = res;
        this.isLoading = false;
      },
      error: () => {
        this.isLoading = false;
      }
    });
  }

  loadJobProviderProfile(): void {
    this.profileService.getProfile().subscribe({
      next: (res) => {
        this.profile = res;
        this.jobProviderService.getJobProviderProfile().subscribe({
          next: (providerProfile) => {
            this.jobProviderProfile = providerProfile;
          },
          complete: () => {
            this.isLoading = false;
          },
          error: () => {
            this.isLoading = false;
          }
        });
      },
      error: () => {
        this.isLoading = false;
      }
    });
  }

  loadJobSeekerProfile(): void {
    this.jobSeekerService.getProfile().subscribe({
      next: (res) => {
        this.jobSeekerProfile = res;
        this.profile = {
          fullName: res.fullName,
          email: res.email,
          userType: 'JobSeeker'
        };
        this.isLoading = false;
      },
      error: () => {
        this.isLoading = false;
      }
    });
  }

  saveJobProvider(): void {
    this.isSaving = true;
    this.successMessage = '';
    this.errorMessage = '';

    this.profileService.updateProfile({ fullName: this.profile.fullName ?? '' }).subscribe({
      next: () => {
        this.jobProviderService.updateJobProviderProfile(this.jobProviderProfile).subscribe({
          next: () => {
            this.successMessage = 'Profile updated successfully.';
          },
          error: () => {
            this.errorMessage = 'Unable to update company profile.';
          },
          complete: () => {
            this.isSaving = false;
          }
        });
      },
      error: () => {
        this.errorMessage = 'Unable to update profile.';
        this.isSaving = false;
      }
    });
  }

  saveJobSeeker(): void {
    this.jobSeekerSubmitAttempted = true;
    if (!this.jobSeekerProfile.fullName?.trim()) {
      return;
    }

    this.isSaving = true;
    this.successMessage = '';
    this.errorMessage = '';

    this.jobSeekerService.updateProfile(this.jobSeekerProfile).subscribe({
      next: () => {
        this.profileService.updateProfile({ fullName: this.jobSeekerProfile.fullName }).subscribe();
        this.successMessage = 'Profile updated successfully.';
        this.jobSeekerSubmitAttempted = false;
      },
      error: () => {
        this.errorMessage = 'Unable to update job seeker profile.';
      },
      complete: () => {
        this.isSaving = false;
      }
    });
  }

  cancel(): void {
    if (this.embedded) {
      this.cancelled.emit();
      return;
    }
    this.router.navigate(['/profile']);
  }
}
