import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { JobProviderService } from '../../../core/services/job-provider.service';
import { ToastService } from '../../../core/services/toast.service';
import { JobProviderSettings } from '../../../core/models/job-provider/job-provider-settings.model';

@Component({
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './job-applications.component.html',
  styleUrls: ['./job-applications.component.scss']
})
export class JobApplicationsComponent implements OnInit {

  applications: any[] = [];
  filteredApplications: any[] = [];
  isLoading = true;
  jobId!: string;

  searchTerm = '';
  statusFilter = 'all';
  updatingIds = new Set<string>();
  aiLoadingIds = new Set<string>();

  settings: JobProviderSettings | null = null;
  settingsLoading = true;
  settingsError = false;

  showInviteModal = false;
  inviteTarget: any = null;
  inviteDifficulty = '';
  inviteSlots: string[] = [];
  inviteQuestionsCount = 0;
  showReportModal = false;
  reportLoading = false;
  reportError = '';
  reportData: any = null;
  reportTarget: any = null;

  constructor(
    private route: ActivatedRoute,
    private service: JobProviderService,
    private toastService: ToastService
  ) {}

  ngOnInit(): void {
    this.jobId = this.route.snapshot.paramMap.get('jobId')!;
    this.loadSettings();
    this.service.getApplicationsByJob(this.jobId).subscribe(res => {
      this.applications = res ?? [];
      this.applyFilters();
      this.isLoading = false;
    });
  }

  loadSettings(): void {
    this.settingsLoading = true;
    this.settingsError = false;
    this.service.getJobProviderSettings().subscribe({
      next: res => {
        this.settings = this.normalizeSettings(res);
        this.resetInviteFields();
      },
      error: () => {
        this.settingsError = true;
        this.toastService.show('Unable to load recruiter settings.');
        this.settingsLoading = false;
      },
      complete: () => {
        this.settingsLoading = false;
      }
    });
  }

  applyFilters(): void {
    const term = this.searchTerm.trim().toLowerCase();
    const status = this.statusFilter;

    this.filteredApplications = this.applications.filter(app => {
      const name = (app.fullName ?? '').toLowerCase();
      const email = (app.email ?? '').toLowerCase();
      const matchesTerm = !term || name.includes(term) || email.includes(term);
      const matchesStatus = status === 'all' || app.status === status;
      return matchesTerm && matchesStatus;
    });
  }

  getStatusClass(status: string): string {
    switch (status) {
      case 'Applied': return 'badge-applied';
      case 'Shortlisted': return 'badge-shortlisted';
      case 'Rejected': return 'badge-rejected';
      default: return 'badge-default';
    }
  }

  getInitials(name: string): string {
    if (!name) return '?';
    const parts = name.trim().split(' ').filter(Boolean);
    const first = parts[0]?.[0] ?? '';
    const last = parts.length > 1 ? parts[parts.length - 1]?.[0] : '';
    return (first + last).toUpperCase();
  }

  isUpdating(app: any): boolean {
    const key = app?.id ?? app?.jobSeekerId;
    return this.updatingIds.has(key);
  }

  updateStatus(app: any, status: string): void {
    if (!this.jobId || !app?.jobSeekerId) return;

    const key = app?.id ?? app?.jobSeekerId;
    if (this.updatingIds.has(key)) return;

    this.updatingIds.add(key);
    this.service.updateApplicationStatus(this.jobId, app.jobSeekerId, status).subscribe({
      next: () => {
        app.status = status;
        this.toastService.show(`Candidate ${status.toLowerCase()} successfully`);
        this.applyFilters();
        this.updatingIds.delete(key);
      },
      error: () => {
        this.toastService.show('Failed to update application status');
        this.updatingIds.delete(key);
      }
    });
  }

  suggestWithAi(app: any): void {
    if (!this.settings?.ai?.enableShortlistSuggestions) {
      this.toastService.show('AI suggestions are disabled in settings.');
      return;
    }

    const key = app?.id ?? app?.jobSeekerId;
    if (this.aiLoadingIds.has(key)) return;

    this.aiLoadingIds.add(key);
    this.service.getAiShortlistSuggestion(this.jobId, app.jobSeekerId).subscribe({
      next: res => {
        app.aiSuggestion = res;
      },
      error: () => {
        this.toastService.show('AI suggestion failed. Try again.');
      },
      complete: () => {
        this.aiLoadingIds.delete(key);
      }
    });
  }

  openInviteModal(app: any): void {
    if (this.settingsLoading) {
      this.toastService.show('Settings are still loading. Please try again.');
      return;
    }

    if (this.settingsError || !this.settings) {
      this.toastService.show('Settings are unavailable. Please refresh.');
      return;
    }

    if ((this.settings.interview.difficultyLevels ?? []).length === 0) {
      this.toastService.show('Add difficulty levels in Settings before inviting.');
      return;
    }

    if (!this.settings.interview.slotCount || this.settings.interview.slotCount < 1) {
      this.toastService.show('Set the number of slots in Settings before inviting.');
      return;
    }

    this.inviteTarget = app;
    this.resetInviteFields();
    this.showInviteModal = true;
  }

  closeInviteModal(): void {
    this.showInviteModal = false;
    this.inviteTarget = null;
  }

  openReportModal(app: any): void {
    if (!this.jobId || !app?.jobSeekerId) {
      this.toastService.show('Interview report not available.');
      return;
    }

    this.reportTarget = app;
    this.reportData = null;
    this.reportError = '';
    this.reportLoading = true;
    this.showReportModal = true;

    this.service.getInterviewReport(this.jobId, app.jobSeekerId).subscribe({
      next: (res) => {
        this.reportData = res;
      },
      error: (err) => {
        if (err?.status === 404) {
          this.reportError = 'Interview not completed yet.';
        } else {
          this.reportError = 'Unable to load interview report.';
        }
      },
      complete: () => {
        this.reportLoading = false;
      }
    });
  }

  closeReportModal(): void {
    this.showReportModal = false;
    this.reportTarget = null;
  }

  sendInvite(): void {
    if (!this.inviteTarget) return;

    const slots = this.inviteSlots
      .filter(Boolean)
      .map(value => new Date(value).toISOString());

    if (!this.inviteDifficulty) {
      this.toastService.show('Please select a difficulty level');
      return;
    }

    if (slots.length === 0) {
      this.toastService.show('Please add at least one time slot');
      return;
    }

    const payload = {
      difficulty: this.inviteDifficulty,
      proposedSlots: slots,
      questionsCount: this.inviteQuestionsCount
    };

    this.service.sendInterviewInvite(this.jobId, this.inviteTarget.jobSeekerId, payload).subscribe({
      next: () => {
        this.toastService.show('Interview invite sent');
        this.inviteTarget.status = 'Shortlisted';
        this.applyFilters();
        this.closeInviteModal();
      },
      error: () => {
        this.toastService.show('Failed to send interview invite');
      }
    });
  }

  trackSlot(index: number): number {
    return index;
  }

  trackSkill(index: number, skill: any): string {
    return `${skill?.skill ?? ''}-${index}`;
  }

  scorePercent(score: number): number {
    if (!Number.isFinite(score)) return 0;
    const percent = (score / 10) * 100;
    return Math.max(0, Math.min(100, percent));
  }

  get difficultyOptions(): string[] {
    return this.settings?.interview?.difficultyLevels ?? [];
  }

  private resetInviteFields(): void {
    if (!this.settings) return;
    const slotCount = Number(this.settings.interview.slotCount ?? 0);
    this.inviteSlots = Array.from({ length: slotCount }, () => '');
    this.inviteDifficulty = this.settings.interview.defaultDifficulty
      || this.settings.interview.difficultyLevels[0]
      || '';
    this.inviteQuestionsCount = Number(this.settings.interview.questionsCount ?? 0);
  }

  private normalizeSettings(res: any): JobProviderSettings {
    return {
      interview: {
        difficultyLevels: Array.isArray(res?.interview?.difficultyLevels)
          ? res.interview.difficultyLevels.filter((x: string) => !!x)
          : [],
        defaultDifficulty: res?.interview?.defaultDifficulty ?? '',
        questionsCount: Number(res?.interview?.questionsCount ?? 0),
        slotDurationMinutes: Number(res?.interview?.slotDurationMinutes ?? 0),
        slotCount: Number(res?.interview?.slotCount ?? 0)
      },
      ai: {
        enableShortlistSuggestions: !!res?.ai?.enableShortlistSuggestions,
        enableInterviewAi: !!res?.ai?.enableInterviewAi,
        endpoint: res?.ai?.endpoint ?? '',
        deployment: res?.ai?.deployment ?? '',
        apiVersion: res?.ai?.apiVersion ?? '',
        enableAvatar: !!res?.ai?.enableAvatar,
        avatarProvider: res?.ai?.avatarProvider ?? ''
      },
      email: {
        inviteSubject: res?.email?.inviteSubject ?? '',
        inviteBody: res?.email?.inviteBody ?? ''
      }
    };
  }
}
