import { Component, Input, Output, EventEmitter, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Invitation, InterviewTimeSlot } from '../../models/job-seeker/interview.model';
import { InterviewService } from '../../services/interview.service';
import { ToastService } from '../../services/toast.service';

@Component({
  selector: 'app-invitation-modal',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="modal-overlay" (click)="closeModal()">
      <div class="modal-shell" (click)="$event.stopPropagation()">
        <div class="modal-header">
          <div>
            <span class="eyebrow">Interview Invitation</span>
            <h2>{{ invitation.companyName || invitation.jobTitle || 'Recruiter' }}</h2>
            <p class="subtitle">Job ID: {{ invitation.jobId }}</p>
          </div>
          <button class="icon-btn" (click)="closeModal()">&times;</button>
        </div>

        <div class="modal-body">
          <div class="status-chip" [ngSwitch]="invitation.status">
            <span *ngSwitchCase="'pending'" class="pending">Pending Response</span>
            <span *ngSwitchCase="'accepted'" class="accepted">Accepted</span>
            <span *ngSwitchCase="'rejected'" class="rejected">Rejected</span>
            <span *ngSwitchDefault class="default">{{ invitation.status }}</span>
          </div>

          <div class="message-card">
            <h4>Message from Recruiter</h4>
            <p>{{ invitation.message || 'You have been invited to an interview. Please choose a time slot.' }}</p>
          </div>

          <div class="timeline">
            <div class="timeline-item">
              <span>Sent</span>
              <strong>{{ formatDate(invitation.createdAt) }}</strong>
            </div>
            <div class="timeline-item" *ngIf="invitation.expiresAt">
              <span>Expires</span>
              <strong [class.expired]="isExpired()">
                {{ formatDate(invitation.expiresAt) }}
                <span *ngIf="isExpired()">(Expired)</span>
              </strong>
            </div>
            <div class="timeline-item" *ngIf="invitation.respondedAt">
              <span>Responded</span>
              <strong>{{ formatDate(invitation.respondedAt) }}</strong>
            </div>
          </div>

          <div class="slot-card" *ngIf="invitation.proposedSlots?.length">
            <h4>Select a time slot</h4>
            <div class="slot-grid">
              <label
                class="slot-option"
                *ngFor="let slot of invitation.proposedSlots"
                [class.active]="isSelectedSlot(slot)">
                <input
                  type="radio"
                  name="slot"
                  [checked]="isSelectedSlot(slot)"
                  (change)="selectSlot(slot)">
                <div>
                  <strong>{{ formatSlot(slot) }}</strong>
                  <span>{{ formatSlotTime(slot) }}</span>
                </div>
              </label>
            </div>
            <p class="slot-hint">Your selection will schedule the interview and notify the recruiter.</p>
          </div>

          <div class="interview-card" *ngIf="invitation.interview">
            <h4>Interview Details</h4>
            <div class="info-row">
              <span>Type</span>
              <strong>{{ invitation.interview.interviewType | titlecase }}</strong>
            </div>
            <div class="info-row" *ngIf="invitation.interview.scheduledDate">
              <span>Scheduled</span>
              <strong>{{ formatDate(invitation.interview.scheduledDate) }}</strong>
            </div>
          </div>
        </div>

        <div class="modal-footer">
          <button class="btn btn-secondary" (click)="closeModal()">Close</button>
          <button class="btn btn-danger" (click)="rejectInvitation()"
                  *ngIf="invitation.status === 'pending' && !isExpired()"
                  [disabled]="isProcessing">
            {{ isProcessing ? 'Processing...' : 'Decline' }}
          </button>
          <button class="btn btn-primary" (click)="acceptInvitation()"
                  *ngIf="invitation.status === 'pending' && !isExpired()"
                  [disabled]="isProcessing">
            {{ isProcessing ? 'Processing...' : 'Accept Interview' }}
          </button>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .modal-overlay {
      position: fixed;
      inset: 0;
      background: rgba(15, 23, 42, 0.6);
      display: flex;
      align-items: center;
      justify-content: center;
      padding: 24px;
      z-index: 2000;
    }

    .modal-shell {
      width: min(640px, 92vw);
      background: #ffffff;
      border-radius: 22px;
      box-shadow: 0 30px 60px rgba(15, 23, 42, 0.3);
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
      gap: 12px;
    }

    .eyebrow {
      text-transform: uppercase;
      letter-spacing: 0.16em;
      font-size: 0.7rem;
      opacity: 0.8;
    }

    .subtitle {
      margin: 4px 0 0;
      font-size: 0.8rem;
      opacity: 0.85;
    }

    .icon-btn {
      background: rgba(255, 255, 255, 0.2);
      border: none;
      width: 34px;
      height: 34px;
      border-radius: 50%;
      color: #ffffff;
      font-size: 1.2rem;
      cursor: pointer;
      display: grid;
      place-items: center;
    }

    .modal-body {
      padding: 22px;
      display: grid;
      gap: 16px;
    }

    .status-chip span {
      display: inline-flex;
      padding: 6px 12px;
      border-radius: 999px;
      font-size: 0.75rem;
      font-weight: 600;
    }

    .status-chip .pending {
      background: #fef3c7;
      color: #92400e;
    }

    .status-chip .accepted {
      background: #dcfce7;
      color: #166534;
    }

    .status-chip .rejected {
      background: #fee2e2;
      color: #b91c1c;
    }

    .status-chip .default {
      background: #e2e8f0;
      color: #475569;
    }

    .message-card {
      background: #f8fafc;
      border-radius: 16px;
      padding: 16px;
      border: 1px solid rgba(148, 163, 184, 0.2);
    }

    .message-card h4 {
      margin: 0 0 8px;
      font-size: 0.9rem;
    }

    .message-card p {
      margin: 0;
      color: #475569;
      font-size: 0.85rem;
      line-height: 1.5;
    }

    .timeline {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(160px, 1fr));
      gap: 12px;
    }

    .timeline-item {
      background: #ffffff;
      border-radius: 14px;
      border: 1px solid rgba(148, 163, 184, 0.2);
      padding: 12px;
      font-size: 0.8rem;
      color: #64748b;
    }

    .timeline-item strong {
      display: block;
      margin-top: 6px;
      color: #0f172a;
    }

    .timeline-item strong.expired {
      color: #b91c1c;
    }

    .interview-card {
      background: #ecfeff;
      border-radius: 16px;
      padding: 16px;
      border: 1px solid rgba(14, 165, 233, 0.2);
    }

    .interview-card h4 {
      margin: 0 0 12px;
    }

    .info-row {
      display: flex;
      align-items: center;
      justify-content: space-between;
      font-size: 0.85rem;
      color: #475569;
    }

    .slot-card {
      background: #f8fafc;
      border-radius: 16px;
      padding: 16px;
      border: 1px solid rgba(148, 163, 184, 0.2);
      display: grid;
      gap: 12px;
    }

    .slot-card h4 {
      margin: 0;
      font-size: 0.9rem;
    }

    .slot-grid {
      display: grid;
      gap: 10px;
    }

    .slot-option {
      display: flex;
      gap: 10px;
      align-items: center;
      padding: 10px;
      border-radius: 12px;
      border: 1px solid rgba(148, 163, 184, 0.25);
      background: #ffffff;
      cursor: pointer;
    }

    .slot-option.active {
      border-color: #0ea5e9;
      background: #ecfeff;
    }

    .slot-option input {
      margin: 0;
    }

    .slot-option strong {
      display: block;
      font-size: 0.85rem;
      color: #0f172a;
    }

    .slot-option span {
      font-size: 0.75rem;
      color: #64748b;
    }

    .slot-hint {
      margin: 0;
      font-size: 0.75rem;
      color: #64748b;
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
    }

    .btn-primary {
      background: linear-gradient(120deg, #14b8a6, #0ea5e9);
      color: #ffffff;
    }

    .btn-secondary {
      background: #e2e8f0;
      color: #1f2937;
    }

    .btn-danger {
      background: #fee2e2;
      color: #b91c1c;
    }
  `]
})
export class InvitationModalComponent implements OnInit {
  @Input() invitation!: Invitation;
  @Output() closed = new EventEmitter<void>();
  @Output() accepted = new EventEmitter<Invitation>();

  isProcessing = false;
  selectedSlot: Date | null = null;

  constructor(
    private interviewService: InterviewService,
    private toastService: ToastService
  ) {}

  ngOnInit(): void {
    if (this.invitation.selectedSlot) {
      this.selectedSlot = this.invitation.selectedSlot;
    } else if ((this.invitation.proposedSlots ?? []).length === 1) {
      this.selectedSlot = this.invitation.proposedSlots![0].start;
    }
  }

  acceptInvitation(): void {
    if ((this.invitation.proposedSlots ?? []).length > 0 && !this.selectedSlot) {
      this.toastService.show('Please select a time slot.');
      return;
    }
    this.isProcessing = true;
    this.interviewService.acceptInvitation(this.invitation.id, this.selectedSlot ?? undefined)
      .subscribe({
        next: () => {
          this.invitation.status = 'accepted';
          this.invitation.respondedAt = new Date();
          if (this.selectedSlot) {
            this.invitation.selectedSlot = this.selectedSlot;
          }
          this.isProcessing = false;
          this.toastService.show('Interview invitation accepted!');
          this.accepted.emit(this.invitation);
        },
        error: () => {
          this.isProcessing = false;
          this.toastService.show('Failed to accept invitation');
        }
      });
  }

  rejectInvitation(): void {
    this.isProcessing = true;
    this.interviewService.rejectInvitation(this.invitation.id)
      .subscribe({
        next: () => {
          this.invitation.status = 'rejected';
          this.isProcessing = false;
          this.toastService.show('Invitation declined');
        },
        error: () => {
          this.isProcessing = false;
          this.toastService.show('Failed to decline invitation');
        }
      });
  }

  closeModal(): void {
    this.closed.emit();
  }

  selectSlot(slot: InterviewTimeSlot): void {
    this.selectedSlot = slot.start;
  }

  isSelectedSlot(slot: InterviewTimeSlot): boolean {
    if (!this.selectedSlot) return false;
    return this.selectedSlot.getTime() === slot.start.getTime();
  }

  isExpired(): boolean {
    if (!this.invitation.expiresAt) return false;
    return new Date() > new Date(this.invitation.expiresAt);
  }

  formatDate(date: Date | string): string {
    const d = typeof date === 'string' ? new Date(date) : date;
    return d.toLocaleDateString('en-US', {
      month: 'short',
      day: 'numeric',
      year: 'numeric',
      hour: '2-digit',
      minute: '2-digit'
    });
  }

  formatSlot(slot: InterviewTimeSlot): string {
    return slot.start.toLocaleDateString('en-US', {
      weekday: 'short',
      month: 'short',
      day: 'numeric'
    });
  }

  formatSlotTime(slot: InterviewTimeSlot): string {
    const start = slot.start.toLocaleTimeString('en-US', {
      hour: '2-digit',
      minute: '2-digit'
    });
    const end = slot.end.toLocaleTimeString('en-US', {
      hour: '2-digit',
      minute: '2-digit'
    });
    return `${start} - ${end}`;
  }
}
