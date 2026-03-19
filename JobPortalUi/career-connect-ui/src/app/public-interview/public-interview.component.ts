import { Component, OnDestroy, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute } from '@angular/router';
import { InterviewService } from '../core/services/interview.service';
import { PublicInterviewInvite, PublicInterviewSession, Interview } from '../core/models/job-seeker/interview.model';
import { VideoInterviewComponent } from '../core/components/video-interview/video-interview.component';

@Component({
  selector: 'app-public-interview',
  standalone: true,
  imports: [CommonModule, VideoInterviewComponent],
  templateUrl: './public-interview.component.html',
  styleUrls: ['./public-interview.component.scss']
})
export class PublicInterviewComponent implements OnInit, OnDestroy {
  token = '';
  invite: PublicInterviewInvite | null = null;
  session: PublicInterviewSession | null = null;
  interviewVm: Interview | null = null;
  selectedSlot: Date | null = null;
  loading = true;
  error = '';
  isAccepting = false;
  isStarting = false;
  showInterview = false;
  completed = false;
  now = new Date();
  private timer: any;

  constructor(
    private route: ActivatedRoute,
    private interviewService: InterviewService
  ) {}

  ngOnInit(): void {
    this.token = this.route.snapshot.queryParamMap.get('token') ?? '';
    if (!this.token) {
      this.error = 'Missing interview token.';
      this.loading = false;
      return;
    }

    this.loadInvite();
    this.timer = setInterval(() => {
      this.now = new Date();
    }, 30000);
  }

  ngOnDestroy(): void {
    if (this.timer) clearInterval(this.timer);
  }

  loadInvite(): void {
    this.loading = true;
    this.error = '';
    this.interviewService.getPublicInvite(this.token).subscribe({
      next: invite => {
        this.invite = invite;
        if (invite.selectedSlot) {
          this.selectedSlot = invite.selectedSlot;
        }
        if (invite.sessionId) {
          this.loadSession();
        } else {
          this.loading = false;
        }
      },
      error: (err) => {
        this.error = err?.error?.detail || err?.error?.message || 'Unable to load interview invite.';
        this.loading = false;
      }
    });
  }

  loadSession(): void {
    this.interviewService.getPublicSession(this.token).subscribe({
      next: session => {
        this.session = session;
        this.buildInterviewVm();
        this.loading = false;
      },
      error: (err) => {
        this.error = err?.error?.detail || err?.error?.message || 'Unable to load interview session.';
        this.loading = false;
      }
    });
  }

  selectSlot(slot: { start: Date }): void {
    this.selectedSlot = slot.start;
  }

  acceptSlot(): void {
    if (!this.selectedSlot) {
      this.error = 'Please select a time slot.';
      return;
    }

    this.isAccepting = true;
    this.error = '';
    this.interviewService.acceptPublicInvite(this.token, this.selectedSlot).subscribe({
      next: session => {
        this.session = session;
        this.invite = this.invite
          ? { ...this.invite, selectedSlot: this.selectedSlot ?? undefined }
          : this.invite;
        this.buildInterviewVm();
        this.isAccepting = false;
      },
      error: (err) => {
        this.error = err?.error?.detail || err?.error?.message || 'Unable to schedule interview.';
        this.isAccepting = false;
      }
    });
  }

  startInterview(): void {
    if (!this.session) return;
    this.isStarting = true;
    this.interviewService.startPublicSession(this.token).subscribe({
      next: () => {
        this.showInterview = true;
        this.isStarting = false;
      },
      error: (err) => {
        this.error = err?.error?.detail || err?.error?.message || 'Unable to start interview.';
        this.isStarting = false;
      }
    });
  }

  handleSubmitted(): void {
    this.showInterview = false;
    this.completed = true;
  }

  get canStart(): boolean {
    if (!this.session?.scheduledStart) return true;
    const start = new Date(this.session.scheduledStart);
    const end = this.session.scheduledEnd ? new Date(this.session.scheduledEnd) : null;
    if (this.now < start) return false;
    if (end && this.now > end) return false;
    return true;
  }

  get slotWindowMessage(): string {
    if (!this.session?.scheduledStart) return '';
    const start = new Date(this.session.scheduledStart);
    const end = this.session.scheduledEnd ? new Date(this.session.scheduledEnd) : null;
    if (this.now < start) {
      return `This interview opens at ${start.toLocaleString()}.`;
    }
    if (end && this.now > end) {
      return `This interview window ended at ${end.toLocaleString()}.`;
    }
    return '';
  }

  private buildInterviewVm(): void {
    if (!this.session || !this.invite) return;
    const questions = (this.session.questions ?? []).map((question, index) => ({
      id: String(index + 1),
      question,
      expectedDuration: 120,
      order: index + 1
    }));

    this.interviewVm = {
      id: this.session.sessionId,
      invitationId: this.invite.inviteId,
      jobSeekerId: '',
      jobId: '',
      jobProviderId: '',
      companyName: this.invite.jobTitle,
      interviewType: 'video-recording',
      status: 'scheduled',
      scheduledDate: this.session.scheduledStart ? new Date(this.session.scheduledStart) : undefined,
      questions,
      createdAt: new Date(),
      updatedAt: new Date()
    };
  }
}
