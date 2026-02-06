import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';

import { ProfileService } from '../../core/services/profile.service';
import { ProfileResponse } from '../../core/models/profile/profile-response.model';
import { AuthService } from '../../core/services/auth.service';
import { JobProviderService } from '../../core/services/job-provider.service';
import { JobSeekerProfileService } from '../../core/services/job-seeker-profile.service';
import { JobProviderProfile } from '../../core/models/job-provider/job-provider-profile.model';
import { JobSeekerProfile } from '../../core/models/job-seeker/job-seeker-profile.model';

@Component({
  selector: 'app-view-profile',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './view-profile.component.html',
  styleUrls: ['./view-profile.component.scss']
})
export class ViewProfileComponent implements OnInit {

  @Input() embedded = false;
  @Output() editRequested = new EventEmitter<void>();

  profile: ProfileResponse | null = null;
  jobProviderProfile: JobProviderProfile | null = null;
  jobSeekerProfile: JobSeekerProfile | null = null;
  userType = '';
  isLoading = true;

  constructor(
    private profileService: ProfileService,
    private router: Router,
    private authService: AuthService,
    private jobProviderService: JobProviderService,
    private jobSeekerService: JobSeekerProfileService
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

  private loadJobProviderProfile(): void {
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

  private loadJobSeekerProfile(): void {
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

  editProfile(): void {
    if (this.embedded) {
      this.editRequested.emit();
      return;
    }
    this.router.navigate(['/profile/edit']);
  }

  logout(): void {
    this.authService.logout();
  }
   
}
