import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { ProfileService } from '../../core/services/profile.service';
import { JobProviderService } from '../../core/services/job-provider.service';
import { JobSeekerProfileService } from '../../core/services/job-seeker-profile.service';
import { ProfileResponse } from '../../core/models/profile/profile-response.model';
import { JobProviderProfile } from '../../core/models/job-provider/job-provider-profile.model';
import { JobSeekerProfile, WorkExperience, EducationRecord, ProjectRecord, CertificationRecord, LanguageRecord } from '../../core/models/job-seeker/job-seeker-profile.model';

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

  skillInput = '';

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
        this.skillInput = res.skills?.join(', ') ?? '';
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
    this.isSaving = true;
    this.successMessage = '';
    this.errorMessage = '';

    this.jobSeekerProfile.skills = this.skillInput
      .split(',')
      .map(x => x.trim())
      .filter(Boolean);

    this.jobSeekerService.updateProfile(this.jobSeekerProfile).subscribe({
      next: () => {
        this.profileService.updateProfile({ fullName: this.jobSeekerProfile.fullName }).subscribe();
        this.successMessage = 'Profile updated successfully.';
      },
      error: () => {
        this.errorMessage = 'Unable to update job seeker profile.';
      },
      complete: () => {
        this.isSaving = false;
      }
    });
  }

  addWork(): void {
    this.jobSeekerProfile.workHistory.push({
      company: '',
      role: '',
      startDate: '',
      endDate: '',
      description: '',
      skills: []
    });
  }

  removeWork(index: number): void {
    this.jobSeekerProfile.workHistory.splice(index, 1);
  }

  addEducation(): void {
    this.jobSeekerProfile.educationHistory.push({
      school: '',
      degree: '',
      field: '',
      graduationYear: ''
    });
  }

  removeEducation(index: number): void {
    this.jobSeekerProfile.educationHistory.splice(index, 1);
  }

  addProject(): void {
    this.jobSeekerProfile.projects.push({
      name: '',
      role: '',
      description: '',
      link: ''
    });
  }

  removeProject(index: number): void {
    this.jobSeekerProfile.projects.splice(index, 1);
  }

  addCertification(): void {
    this.jobSeekerProfile.certifications.push({
      name: '',
      issuer: '',
      year: ''
    });
  }

  removeCertification(index: number): void {
    this.jobSeekerProfile.certifications.splice(index, 1);
  }

  addLanguage(): void {
    this.jobSeekerProfile.languages.push({
      name: '',
      proficiency: ''
    });
  }

  removeLanguage(index: number): void {
    this.jobSeekerProfile.languages.splice(index, 1);
  }

  generateAiSummary(): void {
    const payload = {
      fullName: this.jobSeekerProfile.fullName,
      targetRole: this.jobSeekerProfile.headline || 'Professional',
      experienceYears: this.jobSeekerProfile.experienceYears,
      skills: this.jobSeekerProfile.skills,
      education: this.jobSeekerProfile.education,
      summary: this.jobSeekerProfile.summary,
      workHistory: this.jobSeekerProfile.workHistory,
      projects: this.jobSeekerProfile.projects,
      certifications: this.jobSeekerProfile.certifications,
      atsFriendly: this.jobSeekerProfile.resumeSettings.atsFriendly,
      template: this.jobSeekerProfile.resumeSettings.template
    };

    this.jobSeekerService.generateResumeAi(payload).subscribe({
      next: (summary) => {
        this.jobSeekerProfile.summary = summary;
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
