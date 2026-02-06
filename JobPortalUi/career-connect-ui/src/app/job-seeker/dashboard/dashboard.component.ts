import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { JobSeekerService } from '../../core/services/job-seeker.service';
import { JobSeekerProfileService } from '../../core/services/job-seeker-profile.service';
import { NotificationService } from '../../core/services/notification.service';
import { InterviewService } from '../../core/services/interview.service';
import { ToastService } from '../../core/services/toast.service';
import { ProfileModalComponent } from '../../core/components/profile-modal/profile-modal.component';
import { NotificationPanelComponent } from '../../core/components/notification-panel/notification-panel.component';
import { InvitationModalComponent } from '../../core/components/invitation-modal/invitation-modal.component';
import { VideoInterviewComponent } from '../../core/components/video-interview/video-interview.component';
import { InterviewAnalysisComponent } from '../../core/components/interview-analysis/interview-analysis.component';
import { JobSeekerProfile } from '../../core/models/job-seeker/job-seeker-profile.model';
import { Invitation, Interview, InterviewAnalysis, InterviewTimeSlot } from '../../core/models/job-seeker/interview.model';
import { Notification } from '../../core/models/notification/notification.model';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';

@Component({
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    ProfileModalComponent,
    NotificationPanelComponent,
    InvitationModalComponent,
    VideoInterviewComponent,
    InterviewAnalysisComponent
  ],
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.scss']
})
export class DashboardComponent implements OnInit, OnDestroy {
  isLoadingProfile = true;
  isLoadingStats = true;
  isProfileComplete = false;
  showProfileModal = false;
  showNotifications = false;
  showProfileEditModal = false;
  showInvitationModal = false;
  showVideoInterview = false;
  showAnalysisModal = false;
  isVideoInterviewLoading = false;
  isAnalysisLoading = false;
  analysisError = '';

  // Stats
  totalApplications = 0;
  shortlistedCount = 0;
  pendingInvitations = 0;
  upcomingInterviews = 0;
  unreadNotifications = 0;
  reportsReady = 0;
  profileCompletion = 0;

  // Data
  profile: JobSeekerProfile | null = null;
  profileChecklist: { label: string; complete: boolean }[] = [];
  recentApplications: any[] = [];
  pendingInvitationsList: Invitation[] = [];
  upcomingInterviewsList: Interview[] = [];
  completedInterviewsList: Interview[] = [];
  recentNotifications: Notification[] = [];
  selectedInvitation: Invitation | null = null;
  activeInterview: Interview | null = null;
  activeAnalysis: InterviewAnalysis | null = null;

  private readonly defaultSlotMinutes = 60;
  private readonly slotMatchToleranceMs = 60 * 1000;
  private invitationSlotMap = new Map<string, InterviewTimeSlot>();
  private destroy$ = new Subject<void>();
  private hasAutoPromptedProfile = false;
  private notificationCloseHandler = () => {
    this.showNotifications = false;
  };

  constructor(
    private jobSeekerService: JobSeekerService,
    private jobSeekerProfileService: JobSeekerProfileService,
    private notificationService: NotificationService,
    private interviewService: InterviewService,
    private toastService: ToastService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.loadProfile();
    this.loadStats();
    this.setupNotifications();
    if (typeof window !== 'undefined') {
      window.addEventListener('close-panel', this.notificationCloseHandler);
    }
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
    if (typeof window !== 'undefined') {
      window.removeEventListener('close-panel', this.notificationCloseHandler);
    }
  }

  private loadProfile(): void {
    this.jobSeekerProfileService.getProfile()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (profile) => {
          this.profile = profile;
          this.profileChecklist = this.buildProfileChecklist(profile);
          this.profileCompletion = this.calculateProfileCompletion(this.profileChecklist);
          this.isProfileComplete = this.isProfileValid(profile);
          if (!this.isProfileComplete && !this.hasAutoPromptedProfile) {
            // If first-time, show edit profile modal
            this.hasAutoPromptedProfile = true;
            setTimeout(() => {
              this.showProfileEditModal = true;
            }, 500);
          }
          this.isLoadingProfile = false;
        },
        error: () => {
          this.isLoadingProfile = false;
          if (!this.hasAutoPromptedProfile) {
            this.hasAutoPromptedProfile = true;
            this.showProfileEditModal = true; // Default to edit if error
          }
        }
      });
  }

  private loadStats(): void {
    this.jobSeekerService.getMyApplications()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (applications: any) => {
          const apps = Array.isArray(applications) ? applications : [];
          this.totalApplications = apps.length;
          this.recentApplications = apps.slice(0, 5);
          this.shortlistedCount = apps.filter(app =>
            String(app?.status || '').toLowerCase() === 'shortlisted'
          ).length;
        },
        error: () => {
          this.totalApplications = 0;
          this.shortlistedCount = 0;
          this.recentApplications = [];
        }
      });

    this.interviewService.getInvitations()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (invitations) => {
          const list = Array.isArray(invitations) ? invitations : [];
          this.setInvitationSlotMap(list);
          const pending = list.filter(i => i.status === 'pending');
          this.pendingInvitations = pending.length;
          this.pendingInvitationsList = pending.slice(0, 3);
        },
        error: () => {
          this.pendingInvitations = 0;
          this.pendingInvitationsList = [];
        }
      });

    this.interviewService.getMyInterviews()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (interviews) => {
          const list = Array.isArray(interviews) ? interviews : [];
          const upcoming = list.filter(i => i.status !== 'completed');
          const completed = list.filter(i => i.status === 'completed');
          this.upcomingInterviews = upcoming.length;
          this.upcomingInterviewsList = upcoming.slice(0, 3);
          this.reportsReady = completed.length;
          this.completedInterviewsList = completed.slice(0, 3);
        },
        error: () => {
          this.upcomingInterviews = 0;
          this.reportsReady = 0;
          this.upcomingInterviewsList = [];
          this.completedInterviewsList = [];
        }
      });

    setTimeout(() => {
      this.isLoadingStats = false;
    }, 1000);
  }

  private setupNotifications(): void {
    this.notificationService.loadNotifications();
    this.notificationService.unreadCount$
      .pipe(takeUntil(this.destroy$))
      .subscribe(count => {
        this.unreadNotifications = count;
      });

    this.notificationService.notifications$
      .pipe(takeUntil(this.destroy$))
      .subscribe(notifications => {
        const sorted = [...notifications].sort((a, b) => b.createdAt.getTime() - a.createdAt.getTime());
        this.recentNotifications = sorted.slice(0, 4);
      });
  }

  private isProfileValid(profile: any): boolean {
    if (!profile) return false;
    const hasSkills = Array.isArray(profile.skills) && profile.skills.length > 0;
    const hasEducation = (Array.isArray(profile.educationHistory) && profile.educationHistory.length > 0)
      || !!profile.education;
    const hasExperience = Number(profile.experienceYears ?? 0) > 0
      || (Array.isArray(profile.workHistory) && profile.workHistory.length > 0);
    return !!profile.fullName && !!profile.email && hasSkills && hasEducation && hasExperience;
  }

  private buildProfileChecklist(profile: JobSeekerProfile): { label: string; complete: boolean }[] {
    return [
      {
        label: 'Personal details',
        complete: !!profile.fullName && !!profile.email
      },
      {
        label: 'Headline',
        complete: !!profile.headline
      },
      {
        label: 'Skills',
        complete: Array.isArray(profile.skills) && profile.skills.length > 0
      },
      {
        label: 'Experience',
        complete: Number(profile.experienceYears ?? 0) > 0
          || (Array.isArray(profile.workHistory) && profile.workHistory.length > 0)
      },
      {
        label: 'Education',
        complete: (Array.isArray(profile.educationHistory) && profile.educationHistory.length > 0)
          || !!profile.education
      },
      {
        label: 'Summary',
        complete: !!profile.summary
      }
    ];
  }

  private calculateProfileCompletion(checklist: { label: string; complete: boolean }[]): number {
    if (!checklist.length) return 0;
    const completed = checklist.filter(item => item.complete).length;
    return Math.round((completed / checklist.length) * 100);
  }

  toggleNotifications(): void {
    this.showNotifications = !this.showNotifications;
  }

  openProfileModal(): void {
    this.showProfileModal = true;
  }

  closeProfileModal(): void {
    this.showProfileModal = false;
    this.loadProfile(); // Reload after closing
  }

  editProfile(): void {
    this.hasAutoPromptedProfile = true;
    this.showProfileEditModal = true;
  }

  closeProfileEditModal(): void {
    this.showProfileEditModal = false;
    this.loadProfile();
  }

  openInvitationModal(invitation: Invitation): void {
    this.selectedInvitation = invitation;
    this.showInvitationModal = true;
  }

  closeInvitationModal(): void {
    this.showInvitationModal = false;
    this.selectedInvitation = null;
    this.loadStats();
  }

  handleInvitationAccepted(invitation: Invitation): void {
    this.showInvitationModal = false;
    this.selectedInvitation = null;
    this.loadStats();

    if (invitation?.interview && invitation.interview.interviewType === 'video-recording') {
      this.openVideoInterview(invitation.interview);
    }
  }

  navigateToJobs(): void {
    this.router.navigate(['/job-seeker/jobs']);
  }

  navigateToApplications(): void {
    this.router.navigate(['/job-seeker/applications']);
  }

  navigateToResume(): void {
    this.router.navigate(['/job-seeker/resume']);
  }

  acceptInvitation(invitationId: string, selectedSlot?: Date): void {
    this.interviewService.acceptInvitation(invitationId, selectedSlot)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          this.toastService.show('Interview invitation accepted!');
          this.loadStats();
        },
        error: () => {
          this.toastService.show('Failed to accept invitation');
        }
      });
  }

  openVideoInterview(interview: Interview): void {
    const gate = this.getInterviewGate(interview);
    if (!gate.allowed) {
      this.toastService.show(gate.message || 'Interview recording is only available during your scheduled slot.');
      return;
    }

    if (interview.recordingLink) {
      window.open(interview.recordingLink, '_blank');
      return;
    }

    this.activeInterview = null;
    this.showVideoInterview = true;
    this.isVideoInterviewLoading = true;
    this.interviewService.getInterview(interview.id)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (loaded) => {
          this.activeInterview = loaded;
          this.isVideoInterviewLoading = false;
        },
        error: () => {
          this.toastService.show('Unable to load interview session');
          this.isVideoInterviewLoading = false;
        }
      });
  }

  closeVideoInterview(): void {
    this.showVideoInterview = false;
    this.activeInterview = null;
  }

  handleInterviewSubmitted(interview: Interview): void {
    this.showVideoInterview = false;
    this.activeInterview = null;
    this.loadStats();
    this.openAnalysis(interview);
  }

  openAnalysis(interview: Interview): void {
    this.showAnalysisModal = true;
    this.isAnalysisLoading = true;
    this.analysisError = '';
    this.activeAnalysis = null;

    this.interviewService.getInterviewAnalysis(interview.id)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (analysis) => {
          this.activeAnalysis = analysis;
          this.isAnalysisLoading = false;
        },
        error: () => {
          this.analysisError = 'Analysis is not available yet.';
          this.isAnalysisLoading = false;
        }
      });
  }

  closeAnalysis(): void {
    this.showAnalysisModal = false;
    this.activeAnalysis = null;
    this.analysisError = '';
  }

  getStatusBadgeClass(status: string): string {
    switch (String(status || '').toLowerCase()) {
      case 'accepted': return 'badge-success';
      case 'shortlisted': return 'badge-warning';
      case 'rejected': return 'badge-danger';
      case 'pending': return 'badge-warning';
      case 'applied': return 'badge-info';
      default: return 'badge-info';
    }
  }

  getInitials(name?: string): string {
    if (!name) return '?';
    const parts = name.trim().split(' ').filter(Boolean);
    const first = parts[0]?.[0] ?? '';
    const last = parts.length > 1 ? parts[parts.length - 1]?.[0] : '';
    return (first + last).toUpperCase();
  }

  getNotificationIcon(type: string): string {
    switch (type) {
      case 'shortlist': return 'bi-star-fill';
      case 'invitation': return 'bi-envelope-open';
      case 'interview': return 'bi-camera-video';
      case 'application-status': return 'bi-clipboard-check';
      default: return 'bi-chat-dots';
    }
  }

  private setInvitationSlotMap(invitations: Invitation[]): void {
    this.invitationSlotMap.clear();
    invitations.forEach(inv => {
      if (!inv?.id || !inv.selectedSlot) return;

      const selectedStart = this.coerceDate(inv.selectedSlot);
      if (!selectedStart) return;

      let start = selectedStart;
      let end: Date | null = null;
      const matchingSlot = inv.proposedSlots?.find(slot =>
        Math.abs(slot.start.getTime() - selectedStart.getTime()) <= this.slotMatchToleranceMs
      );

      if (matchingSlot) {
        start = matchingSlot.start;
        end = matchingSlot.end;
      }

      if (!end) {
        end = new Date(start.getTime() + this.defaultSlotMinutes * 60 * 1000);
      }

      this.invitationSlotMap.set(inv.id, { start, end });
    });
  }

  private getInterviewGate(interview: Interview): { allowed: boolean; message?: string } {
    const slot = this.getInterviewSlotWindow(interview);
    if (!slot) return { allowed: true };

    const now = new Date();
    if (now < slot.start) {
      return {
        allowed: false,
        message: `This interview opens at ${this.formatSlotTime(slot.start)}.`
      };
    }

    if (now > slot.end) {
      return {
        allowed: false,
        message: `This interview slot ended at ${this.formatSlotTime(slot.end)}. Please request a new slot.`
      };
    }

    return { allowed: true };
  }

  private getInterviewSlotWindow(interview: Interview): InterviewTimeSlot | null {
    if (interview?.invitationId) {
      const mapped = this.invitationSlotMap.get(interview.invitationId);
      if (mapped) return mapped;
    }

    const scheduledStart = this.coerceDate((interview as any)?.scheduledDate ?? interview?.scheduledDate);
    if (!scheduledStart) return null;

    const scheduledEnd = this.coerceDate(
      (interview as any)?.scheduledEnd ??
      (interview as any)?.scheduledEndDate ??
      (interview as any)?.scheduledEndAt ??
      (interview as any)?.scheduledTo ??
      (interview as any)?.scheduledUntil
    );

    return {
      start: scheduledStart,
      end: scheduledEnd ?? new Date(scheduledStart.getTime() + this.defaultSlotMinutes * 60 * 1000)
    };
  }

  private coerceDate(value: unknown): Date | null {
    if (!value) return null;
    const date = value instanceof Date ? value : new Date(value as any);
    return Number.isNaN(date.getTime()) ? null : date;
  }

  private formatSlotTime(date: Date): string {
    return date.toLocaleString(undefined, {
      month: 'short',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit'
    });
  }
}
