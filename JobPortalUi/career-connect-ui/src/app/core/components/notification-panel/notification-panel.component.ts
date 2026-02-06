import { Component, OnInit, OnDestroy, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { NotificationService } from '../../services/notification.service';
import { Notification } from '../../models/notification/notification.model';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';

@Component({
  selector: 'app-notification-panel',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="notification-panel">
      <div class="notification-header">
        <div>
          <h3>Notifications</h3>
          <p>Shortlist updates and interview alerts</p>
        </div>
        <button class="btn-close" (click)="close()" aria-label="Close">&times;</button>
      </div>

      <div class="notification-controls">
        <button class="btn-small" (click)="markAllAsRead()" *ngIf="unreadCount > 0">
          Mark all as read
        </button>
      </div>

      <div class="notification-list">
        <div *ngIf="notifications.length === 0" class="empty-state">
          <p>No notifications yet</p>
        </div>

        <div *ngFor="let notification of notifications"
             class="notification-item"
             [class.unread]="!notification.read"
             (click)="handleNotificationClick(notification)">
          <div class="notification-badge" [ngSwitch]="notification.type">
            <span *ngSwitchCase="'shortlist'" class="badge-shortlist"><i class="bi bi-star-fill"></i></span>
            <span *ngSwitchCase="'invitation'" class="badge-invitation"><i class="bi bi-envelope-open"></i></span>
            <span *ngSwitchCase="'interview'" class="badge-interview"><i class="bi bi-camera-video"></i></span>
            <span *ngSwitchCase="'application-status'" class="badge-status"><i class="bi bi-clipboard-check"></i></span>
            <span *ngSwitchDefault class="badge-message"><i class="bi bi-chat-dots"></i></span>
          </div>

          <div class="notification-content">
            <h4>{{ notification.title }}</h4>
            <p>{{ notification.message }}</p>
            <small>{{ formatDate(notification.createdAt) }}</small>
          </div>

          <button class="btn-delete" (click)="deleteNotification($event, notification.id)"
                  aria-label="Delete">
            <i class="bi bi-trash"></i>
          </button>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .notification-panel {
      position: fixed;
      right: 24px;
      top: 90px;
      width: 380px;
      max-width: 90vw;
      background: #ffffff;
      border-radius: 18px;
      box-shadow: 0 18px 40px rgba(15, 23, 42, 0.2);
      display: flex;
      flex-direction: column;
      max-height: 70vh;
      z-index: 1000;
    }

    .notification-header {
      padding: 18px;
      border-bottom: 1px solid #e2e8f0;
      display: flex;
      justify-content: space-between;
      align-items: center;
      gap: 12px;
      background: linear-gradient(120deg, #0f766e, #0ea5e9);
      color: #ffffff;
    }

    .notification-header h3 {
      margin: 0;
      font-size: 16px;
      font-weight: 600;
    }

    .notification-header p {
      margin: 4px 0 0;
      font-size: 12px;
      opacity: 0.85;
    }

    .btn-close {
      background: rgba(255, 255, 255, 0.2);
      border: none;
      width: 32px;
      height: 32px;
      border-radius: 50%;
      font-size: 18px;
      cursor: pointer;
      color: #ffffff;
      display: grid;
      place-items: center;
    }

    .notification-controls {
      padding: 10px 16px;
      border-bottom: 1px solid #f1f5f9;
      background: #f8fafc;
    }

    .btn-small {
      background: #e2e8f0;
      border: none;
      padding: 6px 12px;
      border-radius: 999px;
      font-size: 11px;
      cursor: pointer;
      transition: transform 0.2s ease;
      font-weight: 600;
      color: #1f2937;
    }

    .btn-small:hover {
      transform: translateY(-1px);
    }

    .notification-list {
      overflow-y: auto;
      flex: 1;
    }

    .empty-state {
      padding: 32px 20px;
      text-align: center;
      color: #94a3b8;
      font-size: 13px;
    }

    .notification-item {
      display: flex;
      gap: 12px;
      padding: 12px 16px;
      border-bottom: 1px solid #f1f5f9;
      cursor: pointer;
      transition: background 0.2s;
      align-items: flex-start;
    }

    .notification-item:hover {
      background: #f9fafb;
    }

    .notification-item.unread {
      background: #ecfeff;
      font-weight: 500;
    }

    .notification-badge {
      width: 36px;
      height: 36px;
      border-radius: 12px;
      background: rgba(14, 165, 233, 0.12);
      display: grid;
      place-items: center;
      color: #0ea5e9;
      font-size: 16px;
      flex-shrink: 0;
    }

    .badge-shortlist {
      background: rgba(245, 158, 11, 0.2);
      color: #b45309;
    }

    .badge-invitation {
      background: rgba(14, 165, 233, 0.15);
      color: #0284c7;
    }

    .badge-interview {
      background: rgba(16, 185, 129, 0.18);
      color: #047857;
    }

    .badge-status {
      background: rgba(99, 102, 241, 0.15);
      color: #4f46e5;
    }

    .notification-content {
      flex: 1;
      min-width: 0;
    }

    .notification-content h4 {
      margin: 0 0 4px 0;
      font-size: 13px;
      font-weight: 600;
    }

    .notification-content p {
      margin: 0;
      font-size: 12px;
      color: #64748b;
      word-break: break-word;
    }

    .notification-content small {
      color: #94a3b8;
      font-size: 10px;
    }

    .btn-delete {
      background: none;
      border: none;
      cursor: pointer;
      font-size: 14px;
      opacity: 0;
      transition: opacity 0.2s;
      color: #94a3b8;
    }

    .notification-item:hover .btn-delete {
      opacity: 1;
    }

    @media (max-width: 600px) {
      .notification-panel {
        width: calc(100vw - 40px);
        right: 20px;
      }
    }
  `]
})
export class NotificationPanelComponent implements OnInit, OnDestroy {
  @Output() closed = new EventEmitter<void>();
  notifications: Notification[] = [];
  unreadCount = 0;
  private destroy$ = new Subject<void>();

  constructor(private notificationService: NotificationService) {}

  ngOnInit(): void {
    this.notificationService.loadNotifications();

    this.notificationService.notifications$
      .pipe(takeUntil(this.destroy$))
      .subscribe(notifications => {
        this.notifications = notifications;
        this.unreadCount = notifications.filter(n => !n.read).length;
      });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  markAllAsRead(): void {
    this.notificationService.markAllAsRead()
      .pipe(takeUntil(this.destroy$))
      .subscribe(() => {
        this.notificationService.loadNotifications();
      });
  }

  deleteNotification(event: Event, notificationId: string): void {
    event.stopPropagation();
    this.notificationService.deleteNotification(notificationId)
      .pipe(takeUntil(this.destroy$))
      .subscribe(() => {
        this.notificationService.loadNotifications();
      });
  }

  handleNotificationClick(notification: Notification): void {
    if (!notification.read) {
      this.notificationService.markAsRead(notification.id)
        .pipe(takeUntil(this.destroy$))
        .subscribe(() => {
          this.notificationService.loadNotifications();
        });
    }

    if (notification.actionLink) {
      window.location.href = notification.actionLink;
    }
  }

  close(): void {
    this.closed.emit();
    const event = new CustomEvent('close-panel');
    window.dispatchEvent(event);
  }

  formatDate(date: Date): string {
    const now = new Date();
    const diffMs = now.getTime() - date.getTime();
    const diffMins = Math.floor(diffMs / 60000);
    const diffHours = Math.floor(diffMs / 3600000);
    const diffDays = Math.floor(diffMs / 86400000);

    if (diffMins < 1) return 'Just now';
    if (diffMins < 60) return `${diffMins}m ago`;
    if (diffHours < 24) return `${diffHours}h ago`;
    if (diffDays < 7) return `${diffDays}d ago`;

    return date.toLocaleDateString();
  }
}
