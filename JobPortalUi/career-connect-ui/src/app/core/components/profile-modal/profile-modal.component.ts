import { Component, Input, Output, EventEmitter, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { JobSeekerProfile } from '../../models/job-seeker/job-seeker-profile.model';
import { JobSeekerProfileService } from '../../services/job-seeker-profile.service';
import { ToastService } from '../../services/toast.service';

@Component({
  selector: 'app-profile-modal',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="modal-overlay" (click)="closeModal()">
      <div class="modal-shell" (click)="$event.stopPropagation()">
        <div class="modal-header">
          <div>
            <span class="eyebrow">Profile</span>
            <h2>{{ mode === 'view' ? 'Profile Snapshot' : 'Edit Profile' }}</h2>
            <p class="subtitle">
              {{ mode === 'view' ? 'What recruiters see about you.' : 'Keep your details current and complete.' }}
            </p>
          </div>
          <button class="icon-btn" (click)="closeModal()">&times;</button>
        </div>

        <div class="modal-body">
          <div class="loading-state" *ngIf="isLoading">
            Loading profile...
          </div>

          <ng-container *ngIf="!isLoading">
            <section *ngIf="mode === 'view'" class="profile-summary">
              <div class="summary-avatar">{{ profile.fullName?.[0] || '?' }}</div>
              <div>
                <h3>{{ profile.fullName || 'Profile' }}</h3>
                <p>{{ profile.headline || 'Add a headline to highlight your role.' }}</p>
                <div class="summary-meta">
                  <span *ngIf="profile.location">
                    <i class="bi bi-geo-alt"></i>
                    {{ profile.location }}
                  </span>
                  <span>
                    <i class="bi bi-briefcase"></i>
                    {{ profile.experienceYears || 0 }} years experience
                  </span>
                </div>
              </div>
            </section>

            <div *ngIf="mode === 'view'" class="profile-grid">
              <section class="profile-card">
                <h3>Personal Info</h3>
                <div class="info-row">
                  <span>Full Name</span>
                  <strong>{{ profile.fullName || '-' }}</strong>
                </div>
                <div class="info-row">
                  <span>Email</span>
                  <strong>{{ profile.email || '-' }}</strong>
                </div>
                <div class="info-row">
                  <span>Phone</span>
                  <strong>{{ profile.phone || '-' }}</strong>
                </div>
                <div class="info-row">
                  <span>Gender</span>
                  <strong>{{ profile.gender || '-' }}</strong>
                </div>
                <div class="info-row">
                  <span>Location</span>
                  <strong>{{ profile.location || '-' }}</strong>
                </div>
              </section>

              <section class="profile-card">
                <h3>Skills</h3>
                <div class="tag-list" *ngIf="profile.skills?.length">
                  <span class="tag" *ngFor="let skill of profile.skills">{{ skill }}</span>
                </div>
                <p class="empty" *ngIf="!profile.skills?.length">Add skills to improve match quality.</p>
              </section>

              <section class="profile-card full">
                <h3>Professional Summary</h3>
                <p class="text-block">
                  {{ profile.summary || 'Add a professional summary for recruiters.' }}
                </p>
              </section>

              <section class="profile-card full">
                <h3>Work Experience</h3>
                <div class="timeline" *ngIf="profile.workHistory?.length">
                  <div class="timeline-item" *ngFor="let work of profile.workHistory">
                    <h4>{{ work.role }} · {{ work.company }}</h4>
                    <small>{{ work.startDate }} - {{ work.endDate || 'Present' }}</small>
                    <p>{{ work.description }}</p>
                  </div>
                </div>
                <p class="empty" *ngIf="!profile.workHistory?.length">Add your recent experience.</p>
              </section>

              <section class="profile-card">
                <h3>Education</h3>
                <div *ngIf="profile.educationHistory?.length">
                  <div class="mini-row" *ngFor="let edu of profile.educationHistory">
                    <strong>{{ edu.degree }} · {{ edu.field }}</strong>
                    <span>{{ edu.school }} · {{ edu.graduationYear }}</span>
                  </div>
                </div>
                <p class="empty" *ngIf="!profile.educationHistory?.length">Add education highlights.</p>
              </section>

              <section class="profile-card">
                <h3>Projects</h3>
                <div *ngIf="profile.projects?.length">
                  <div class="mini-row" *ngFor="let project of profile.projects">
                    <strong>{{ project.name }}</strong>
                    <span>{{ project.role }}</span>
                  </div>
                </div>
                <p class="empty" *ngIf="!profile.projects?.length">Showcase key projects.</p>
              </section>

              <section class="profile-card">
                <h3>Certifications</h3>
                <div *ngIf="profile.certifications?.length">
                  <div class="mini-row" *ngFor="let cert of profile.certifications">
                    <strong>{{ cert.name }}</strong>
                    <span>{{ cert.issuer }} · {{ cert.year }}</span>
                  </div>
                </div>
                <p class="empty" *ngIf="!profile.certifications?.length">Add relevant certifications.</p>
              </section>

              <section class="profile-card">
                <h3>Languages</h3>
                <div *ngIf="profile.languages?.length">
                  <div class="mini-row" *ngFor="let lang of profile.languages">
                    <strong>{{ lang.name }}</strong>
                    <span>{{ lang.proficiency }}</span>
                  </div>
                </div>
                <p class="empty" *ngIf="!profile.languages?.length">Add language fluency.</p>
              </section>

              <section class="profile-card">
                <h3>Resume Settings</h3>
                <div class="info-row">
                  <span>Template</span>
                  <strong>{{ profile.resumeSettings?.template || 'Clean' }}</strong>
                </div>
                <div class="info-row">
                  <span>ATS Friendly</span>
                  <strong>{{ profile.resumeSettings?.atsFriendly ? 'Yes' : 'No' }}</strong>
                </div>
              </section>
            </div>

            <div *ngIf="mode === 'edit'" class="edit-grid">
              <section class="form-section">
                <h3>Basic Information</h3>
                <div class="form-row">
                  <div>
                    <label>Full Name *</label>
                    <input
                      type="text"
                      name="fullName"
                      required
                      [(ngModel)]="editProfile.fullName"
                      class="form-control"
                      #fullName="ngModel"
                      [class.is-invalid]="fullName.invalid && (fullName.touched || formSubmitted)">
                    <p class="field-error" *ngIf="fullName.invalid && (fullName.touched || formSubmitted)">
                      Full name is required
                    </p>
                  </div>
                  <div>
                    <label>Email *</label>
                    <input
                      type="email"
                      name="email"
                      [(ngModel)]="editProfile.email"
                      class="form-control"
                      disabled>
                  </div>
                </div>
                <div class="form-row">
                  <div>
                    <label>Phone</label>
                    <input
                      type="tel"
                      name="phone"
                      [(ngModel)]="editProfile.phone"
                      class="form-control">
                  </div>
                  <div>
                    <label>Gender</label>
                    <input
                      type="text"
                      name="gender"
                      [(ngModel)]="editProfile.gender"
                      class="form-control">
                  </div>
                </div>
              </section>
            </div>
          </ng-container>
        </div>

        <div class="modal-footer">
          <button class="btn btn-secondary" (click)="closeModal()">
            {{ mode === 'view' ? 'Close' : 'Cancel' }}
          </button>
          <button class="btn btn-primary" (click)="editMode()" *ngIf="mode === 'view'">
            Edit Profile
          </button>
          <button class="btn btn-primary" (click)="saveProfile()" [disabled]="isSaving" *ngIf="mode === 'edit'">
            {{ isSaving ? 'Saving...' : 'Save Changes' }}
          </button>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .modal-overlay {
      position: fixed;
      inset: 0;
      background: rgba(15, 23, 42, 0.65);
      display: flex;
      align-items: center;
      justify-content: center;
      padding: 24px;
      z-index: 2000;
    }

    .modal-shell {
      width: min(960px, 94vw);
      max-height: 92vh;
      background: #ffffff;
      border-radius: 24px;
      box-shadow: 0 30px 60px rgba(15, 23, 42, 0.35);
      overflow: hidden;
      display: flex;
      flex-direction: column;
    }

    .modal-header {
      padding: 20px 24px;
      background: linear-gradient(120deg, #0f766e, #0ea5e9);
      color: #ffffff;
      display: flex;
      align-items: center;
      justify-content: space-between;
      gap: 16px;
    }

    .eyebrow {
      text-transform: uppercase;
      letter-spacing: 0.16em;
      font-size: 0.7rem;
      opacity: 0.75;
    }

    .modal-header h2 {
      margin: 6px 0 0;
      font-size: 1.4rem;
    }

    .subtitle {
      margin: 4px 0 0;
      font-size: 0.85rem;
      opacity: 0.85;
    }

    .icon-btn {
      background: rgba(255, 255, 255, 0.2);
      border: none;
      width: 36px;
      height: 36px;
      border-radius: 50%;
      color: #ffffff;
      font-size: 1.2rem;
      cursor: pointer;
    }

    .modal-body {
      padding: 24px;
      overflow-y: auto;
    }

    .loading-state {
      padding: 40px 0;
      text-align: center;
      color: #64748b;
    }

    .profile-summary {
      display: flex;
      gap: 16px;
      align-items: center;
      padding: 16px;
      border-radius: 18px;
      background: #f8fafc;
      border: 1px solid rgba(148, 163, 184, 0.2);
      margin-bottom: 20px;
    }

    .summary-avatar {
      width: 56px;
      height: 56px;
      border-radius: 16px;
      background: linear-gradient(135deg, #0f766e, #14b8a6);
      color: #ffffff;
      display: grid;
      place-items: center;
      font-weight: 700;
    }

    .summary-meta {
      display: flex;
      gap: 12px;
      flex-wrap: wrap;
      font-size: 0.8rem;
      color: #64748b;
    }

    .summary-meta i {
      margin-right: 6px;
      color: #0f766e;
    }

    .profile-grid {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(240px, 1fr));
      gap: 16px;
    }

    .profile-card {
      background: #ffffff;
      border-radius: 18px;
      border: 1px solid rgba(148, 163, 184, 0.2);
      padding: 16px;
      display: flex;
      flex-direction: column;
      gap: 12px;
    }

    .profile-card.full {
      grid-column: 1 / -1;
    }

    .profile-card h3 {
      margin: 0;
      font-size: 1rem;
    }

    .info-row {
      display: flex;
      align-items: center;
      justify-content: space-between;
      font-size: 0.85rem;
      color: #64748b;
    }

    .info-row strong {
      color: #0f172a;
    }

    .tag-list {
      display: flex;
      flex-wrap: wrap;
      gap: 8px;
    }

    .tag {
      background: #e2f3f3;
      color: #0f766e;
      padding: 6px 12px;
      border-radius: 999px;
      font-size: 0.75rem;
      font-weight: 600;
    }

    .text-block {
      margin: 0;
      color: #475569;
      line-height: 1.6;
    }

    .timeline {
      display: grid;
      gap: 12px;
    }

    .timeline-item {
      padding: 12px;
      border-radius: 14px;
      background: #f8fafc;
      border: 1px solid rgba(148, 163, 184, 0.2);
    }

    .timeline-item h4 {
      margin: 0 0 6px;
      font-size: 0.95rem;
    }

    .timeline-item p {
      margin: 6px 0 0;
      font-size: 0.85rem;
      color: #64748b;
    }

    .mini-row {
      display: flex;
      flex-direction: column;
      gap: 4px;
      font-size: 0.85rem;
      color: #475569;
    }

    .empty {
      margin: 0;
      font-size: 0.8rem;
      color: #94a3b8;
    }

    .edit-grid {
      display: flex;
      flex-direction: column;
      gap: 18px;
    }

    .form-section {
      border: 1px solid rgba(148, 163, 184, 0.2);
      border-radius: 18px;
      padding: 16px;
      background: #f8fafc;
    }

    .form-section h3 {
      margin: 0 0 12px;
    }

    .form-section .muted {
      margin: 4px 0 0;
      font-size: 0.82rem;
      color: #64748b;
    }

    .mode-toggle {
      display: flex;
      gap: 12px;
      flex-wrap: wrap;
      margin-top: 12px;
    }

    .mode-chip {
      border: 1px solid rgba(148, 163, 184, 0.4);
      background: #ffffff;
      color: #475569;
      padding: 8px 16px;
      border-radius: 999px;
      font-size: 0.85rem;
      cursor: pointer;
      transition: all 0.2s ease;
    }

    .mode-chip.active {
      background: #0f766e;
      border-color: #0f766e;
      color: #ffffff;
    }

    .import-section {
      background: #ffffff;
    }

    .import-grid {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(240px, 1fr));
      gap: 16px;
    }

    .import-card {
      border: 1px dashed rgba(148, 163, 184, 0.5);
      border-radius: 16px;
      padding: 12px;
      background: #f8fafc;
    }

    .import-label {
      font-weight: 600;
      margin-bottom: 8px;
      display: block;
      color: #0f172a;
    }

    .import-file {
      margin: 8px 0 0;
      font-size: 0.8rem;
      color: #0f766e;
    }

    .import-hint {
      margin: 8px 0 0;
      font-size: 0.78rem;
      color: #94a3b8;
    }

    .import-actions {
      display: flex;
      align-items: center;
      justify-content: space-between;
      gap: 12px;
      margin-top: 16px;
      flex-wrap: wrap;
    }

    .import-error {
      color: #b91c1c;
      font-size: 0.8rem;
    }

    .section-header {
      display: flex;
      align-items: center;
      justify-content: space-between;
      gap: 12px;
      margin-bottom: 12px;
    }

    .form-row {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
      gap: 12px;
      margin-bottom: 12px;
    }

    label {
      font-size: 0.8rem;
      color: #475569;
      margin-bottom: 6px;
      display: block;
    }

    .form-control,
    .form-select {
      width: 100%;
      padding: 10px 12px;
      border-radius: 12px;
      border: 1px solid rgba(148, 163, 184, 0.4);
      font-size: 0.85rem;
      font-family: inherit;
      background: #ffffff;
    }

    .form-control.is-invalid,
    .form-select.is-invalid {
      border-color: #ef4444;
      box-shadow: 0 0 0 3px rgba(239, 68, 68, 0.12);
    }

    .field-error {
      color: #dc2626;
      font-size: 0.75rem;
      margin: 6px 0 0;
    }

    .repeat-card {
      background: #ffffff;
      border-radius: 14px;
      padding: 12px;
      border: 1px solid rgba(148, 163, 184, 0.2);
      margin-bottom: 12px;
      display: grid;
      gap: 10px;
    }

    .repeat-header {
      display: flex;
      align-items: center;
      justify-content: space-between;
      font-size: 0.85rem;
    }

    .toggle {
      display: flex;
      flex-direction: column;
      gap: 6px;
      align-items: flex-start;
    }

    .modal-footer {
      padding: 16px 24px;
      border-top: 1px solid rgba(148, 163, 184, 0.2);
      display: flex;
      justify-content: flex-end;
      gap: 12px;
    }

    .btn {
      border: none;
      border-radius: 999px;
      padding: 10px 18px;
      font-weight: 600;
      cursor: pointer;
      transition: transform 0.2s ease;
    }

    .btn-primary {
      background: linear-gradient(120deg, #14b8a6, #0ea5e9);
      color: #ffffff;
    }

    .btn-secondary {
      background: #e2e8f0;
      color: #1f2937;
    }

    .btn-secondary:hover,
    .btn-primary:hover {
      transform: translateY(-1px);
    }

    .btn-danger-small {
      background: #fee2e2;
      color: #b91c1c;
      border: none;
      border-radius: 999px;
      padding: 4px 10px;
      font-size: 0.7rem;
      cursor: pointer;
    }

    @media (max-width: 720px) {
      .modal-header {
        flex-direction: column;
        align-items: flex-start;
      }

      .modal-footer {
        flex-direction: column;
      }
    }
  `]
})
export class ProfileModalComponent implements OnInit {
  @Input() initialMode: 'view' | 'edit' = 'view';
  @Output() closed = new EventEmitter<void>();

  mode: 'view' | 'edit' = 'view';
  profile: JobSeekerProfile = this.getEmptyProfile();
  editProfile: JobSeekerProfile = this.getEmptyProfile();
  isSaving = false;
  isLoading = true;
  formSubmitted = false;

  constructor(
    private jobSeekerService: JobSeekerProfileService,
    private toastService: ToastService
  ) {}

  ngOnInit(): void {
    this.mode = this.initialMode;
    this.loadProfile();
  }

  private loadProfile(): void {
    this.jobSeekerService.getProfile().subscribe({
      next: (profile) => {
        this.profile = this.normalizeProfile(profile);
        this.editProfile = JSON.parse(JSON.stringify(this.profile));
        this.isLoading = false;
      },
      error: () => {
        this.isLoading = false;
        this.toastService.show('Failed to load profile');
      }
    });
  }

  editMode(): void {
    this.mode = 'edit';
    this.formSubmitted = false;
  }

  saveProfile(): void {
    this.formSubmitted = true;
    if (!this.editProfile.fullName?.trim()) {
      return;
    }

    this.isSaving = true;
    this.jobSeekerService.updateProfile(this.editProfile).subscribe({
      next: () => {
        this.profile = JSON.parse(JSON.stringify(this.editProfile));
        this.mode = 'view';
        this.isSaving = false;
        this.formSubmitted = false;
        this.toastService.show('Profile updated successfully');
      },
      error: () => {
        this.isSaving = false;
        this.toastService.show('Failed to update profile');
      }
    });
  }

  closeModal(): void {
    this.closed.emit();
  }

  private normalizeProfile(profile: JobSeekerProfile): JobSeekerProfile {
    return {
      ...profile,
      workHistory: Array.isArray(profile.workHistory) ? profile.workHistory : [],
      educationHistory: Array.isArray(profile.educationHistory) ? profile.educationHistory : [],
      projects: Array.isArray(profile.projects) ? profile.projects : [],
      certifications: Array.isArray(profile.certifications) ? profile.certifications : [],
      languages: Array.isArray(profile.languages) ? profile.languages : [],
      resumeSettings: profile.resumeSettings ?? {
        atsFriendly: true,
        template: 'Clean'
      }
    };
  }

  private getEmptyProfile(): JobSeekerProfile {
    return {
      fullName: '',
      email: '',
      phone: '',
      gender: '',
      headline: '',
      summary: '',
      location: '',
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
  }
}
